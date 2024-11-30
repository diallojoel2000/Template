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
        public static void CommandCreate(Type entity, IEnumerable<IProperty> properties)
        {
            var builder = new StringBuilder();
            builder.AppendLine("using Application.Common.Interfaces;");
            builder.AppendLine("using Domain.Entities;");
            builder.AppendLine();
            builder.AppendLine($"namespace Application.{entity.Name.Plural()}.Commands");
            builder.AppendLine();
            builder.AppendLine($"public record Create{entity.Name}Command : IRequest<long>");
            builder.AppendLine("{");
            foreach (var property in properties)
            {
                builder.Append("\t").Append($"public {TypeMap(property.ClrType.Name)} {property.Name}").Append(" { get; init; }");
                builder.AppendLine();
            }
            builder.AppendLine("}");

            var content = builder.ToString();
        }

        public static string TypeMap(string type) => type switch
        {
            "Int32" => "int",
            "Int64" => "long",
            "String" => type.ToLower(),
            _=>type
        };
    }
}
