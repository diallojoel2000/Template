using Application.Common.Interfaces;

namespace Application.Common.Mappings;
public interface IMapFrom<T>
{
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
    void Mapping(Profile profile, IEncryptionService encryptionService) => profile.CreateMap(typeof(T), GetType());
    
}
