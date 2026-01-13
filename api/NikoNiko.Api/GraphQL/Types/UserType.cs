using HotChocolate.Authorization;

using NikoNiko.Core.Models;

namespace NikoNiko.Api.GraphQL.Types;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(u => u.Id).Type<NonNullType<IdType>>();
        descriptor.Field(u => u.Email).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.Name).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.AvatarUrl).Type<StringType>();
        descriptor.Field(u => u.IsSuperAdmin).Type<NonNullType<BooleanType>>();
        descriptor.Field(u => u.CreatedAt).Type<NonNullType<DateTimeType>>();

        // TODO: Add resolver for Teams via TeamUsers
    }
}