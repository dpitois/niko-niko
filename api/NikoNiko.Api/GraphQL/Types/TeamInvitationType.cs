using NikoNiko.Core.Models;

namespace NikoNiko.Api.GraphQL.Types;

public class TeamInvitationType : ObjectType<TeamInvitation>
{
    protected override void Configure(IObjectTypeDescriptor<TeamInvitation> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(i => i.Id).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.TeamId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.CreatorUserId).Type<NonNullType<IdType>>();
        descriptor.Field(i => i.Token).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.ExpirationDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(i => i.Status).Type<NonNullType<StringType>>();
        descriptor.Field(i => i.CreatedAt).Type<NonNullType<DateTimeType>>();

        descriptor.Field(i => i.Team).Type<TeamType>();
        descriptor.Field(i => i.CreatorUser).Type<UserType>();
    }
}