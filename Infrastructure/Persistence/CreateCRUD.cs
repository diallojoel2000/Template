using Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text;

namespace Infrastructure.Persistence
{
    public static class CreateCRUD
    {
        public static string Plural(this string text) 
        {
            if(text.EndsWith("y"))
            {
                text = $"{text.Substring(0, text.Length - 1)}ies";
            }
            else
            {
                text = $"{text}s";
            }
            return text;
        }
        public static void CommandCreate(Type entity, IEnumerable<IProperty> properties, string folderPath)
        {
            var builder = new StringBuilder();
            var className = $"Create{entity.Name}Command";
            #region CreateRecord
            
            builder.AppendLine("using Application.Common.Interfaces;");
            builder.AppendLine("using Domain.Entities;");
            builder.AppendLine();
            builder.AppendLine($"namespace Application.{entity.Name.Plural()};");
            builder.AppendLine();
            builder.AppendLine($"public record {className} : IRequest<long>");
            builder.AppendLine("{");
            foreach (var property in properties)
            {
                if (IsAuditableEntity(property.Name))
                { continue; }
                builder.Append("\t").Append($"public {TypeMap(property.ClrType.Name)} {property.Name}").Append(" { get; init; }");
                builder.AppendLine();
            }
            builder.AppendLine("}"); 
            #endregion

            builder.AppendLine();

            #region Create Handler
            builder.AppendLine($"internal class {className}Handler : IRequestHandler<{className},long>");
            builder.AppendLine("{");
            builder.AppendLine("\tprivate readonly IApplicationDbContext _context;");
            builder.AppendLine($"\tpublic {className}Handler(IApplicationDbContext context)");
            builder.AppendLine("\t{");
            builder.AppendLine("\t\t_context = context;");
            builder.AppendLine("\t}");
            builder.AppendLine($"\tpublic async Task<long> Handle({className} request, CancellationToken cancellationToken)");
            builder.AppendLine("\t{");
            builder.AppendLine($"\t\tvar {entity.Name.ToLower()} = new {entity.Name}");
            builder.AppendLine("\t\t{");
            foreach (var property in properties)
            {
                if(IsAuditableEntity(property.Name))
                { continue; }
                builder.AppendLine($"\t\t\t{property.Name} = request.{property.Name},");
            }
            builder.AppendLine("\t\t};");
            builder.AppendLine($"\t_context.{entity.Name.Plural()}.Add({entity.Name.ToLower()});");
            builder.AppendLine("\tawait _context.SaveChangesAsync(cancellationToken);");
            builder.AppendLine($"\treturn {entity.Name.ToLower()}.Id;");
            builder.AppendLine("}");
            #endregion

            string fileName = $"{className}.cs";
            Directory.CreateDirectory($"{folderPath}/Create");
            string fullRelativePathToFile = Path.Combine(folderPath,"Create", fileName);

            File.WriteAllText(fullRelativePathToFile, builder.ToString());
            CommandCreateValidator(className,entity,properties, $"{folderPath}/Create");
        }
        private static void CommandCreateValidator(string className, Type entity, IEnumerable<IProperty> properties, string folderPath)
        {
            var builder = new StringBuilder();
            builder.AppendLine("using Domain.Entities;");
            builder.AppendLine();
            builder.AppendLine($"namespace Application.{entity.Name.Plural()} ;");
            builder.AppendLine();

            builder.AppendLine($"public class {className}Validator: AbstractValidator<{className}>");
            builder.AppendLine("{");
            builder.AppendLine($"\tpublic {className}Validator()");
            builder.AppendLine("\t{");
            foreach (var property in properties)
            {
                if (IsAuditableEntity(property.Name))
                { continue; }
                if(TypeMap(property.ClrType.Name)=="string" && !property.IsNullable)
                {
                    var maxLength = property.GetMaxLength();
                    builder.Append($"\t\tRuleFor(m=>m.{property.Name}).NotEmpty()");
                    if(maxLength.HasValue)
                    {
                        builder.Append($".MaximumLength({maxLength.Value});");
                    }
                    builder.Append(";");
                    builder.AppendLine();
                }
                
            }

            builder.AppendLine("\t}");
            builder.AppendLine("}");

            string fileName = $"{className}Validator.cs";
            
            string fullRelativePathToFile = Path.Combine(folderPath, fileName);

            File.WriteAllText(fullRelativePathToFile, builder.ToString());
        }

        private static string TypeMap(string type) => type switch
        {
            "Int32" => "int",
            "Int64" => "long",
            "String" => type.ToLower(),
            "Boolean" => "bool",
            _ =>type
        };
        private static string GenerateRecord(Type entity, IEnumerable<IProperty> properties)
        {
            var builder = new StringBuilder();
            foreach (var property in properties)
            {
                builder.Append("\t").Append($"public {TypeMap(property.ClrType.Name)} {property.Name}").Append(" { get; init; }");
                builder.AppendLine();
            }
            return builder.ToString();
        }
        private static bool IsAuditableEntity(string propName)
        {
            Type baseEntity = typeof(BaseAuditableEntity);
            var properties = baseEntity.GetProperties();

            foreach (var property in properties)
            {
                if(property.Name ==propName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
