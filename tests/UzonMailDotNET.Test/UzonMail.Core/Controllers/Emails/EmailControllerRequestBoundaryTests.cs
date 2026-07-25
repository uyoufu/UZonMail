using Microsoft.AspNetCore.Mvc;
using UzonMail.CorePlugin.Controllers.Emails;
using UzonMail.CorePlugin.Controllers.Emails.DTOs;
using UzonMail.DB.SQL.Base;

namespace UzonMailDotNET.Test.UzonMail.Core.Controllers.Emails;

/// <summary>
/// 防止邮件控制器重新将数据库实体暴露为请求模型。
/// </summary>
[TestClass]
public sealed class EmailControllerRequestBoundaryTests
{
    [TestMethod]
    public void FromBodyParameters_DoNotUseDatabaseEntities()
    {
        var controllerNamespace = typeof(EmailBoxController).Namespace;
        var entityParameters = typeof(EmailBoxController)
            .Assembly.GetTypes()
            .Where(type => type.Namespace == controllerNamespace && type.Name.EndsWith("Controller"))
            .SelectMany(type => type.GetMethods())
            .SelectMany(method => method.GetParameters())
            .Where(parameter => parameter.GetCustomAttributes(typeof(FromBodyAttribute), true).Length > 0)
            .Select(parameter => UnwrapCollectionType(parameter.ParameterType))
            .Where(type => typeof(SqlId).IsAssignableFrom(type))
            .ToList();

        Assert.IsEmpty(entityParameters);
    }

    [TestMethod]
    public void EmailDtos_DoNotContainDatabaseEntityProperties()
    {
        var dtoNamespace = typeof(CreateOutboxDto).Namespace;
        var entityProperties = typeof(CreateOutboxDto)
            .Assembly.GetTypes()
            .Where(type => type.Namespace == dtoNamespace && type.IsPublic)
            .SelectMany(type => type.GetProperties())
            .Select(property => new
            {
                Property = property,
                Type = UnwrapCollectionType(property.PropertyType),
            })
            .Where(info => typeof(SqlId).IsAssignableFrom(info.Type))
            .Select(info => $"{info.Property.DeclaringType?.Name}.{info.Property.Name}")
            .ToList();

        Assert.IsEmpty(entityProperties);
    }

    private static Type UnwrapCollectionType(Type type)
    {
        var unwrappedType = Nullable.GetUnderlyingType(type) ?? type;
        if (unwrappedType.IsArray)
            return unwrappedType.GetElementType()!;

        return unwrappedType.IsGenericType
            && typeof(System.Collections.IEnumerable).IsAssignableFrom(unwrappedType)
            ? unwrappedType.GetGenericArguments()[0]
            : unwrappedType;
    }
}
