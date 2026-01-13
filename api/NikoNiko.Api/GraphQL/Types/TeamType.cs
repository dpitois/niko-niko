using NikoNiko.Core.Models;

namespace NikoNiko.Api.GraphQL.Types;

public class TeamType : ObjectType<Team>
{
    protected override void Configure(IObjectTypeDescriptor<Team> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
        descriptor.Field(t => t.Name).Type<NonNullType<StringType>>();
        descriptor.Field(t => t.CreatedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(t => t.AdminId).Type<NonNullType<IdType>>();

        descriptor.Field(t => t.Admin)
            .Type<NonNullType<UserType>>()
            .Description("The administrator of the team.");

        descriptor.Field(t => t.Sprints)
            .Type<NonNullType<ListType<NonNullType<SprintType>>>>()
            .UseFiltering()
            .UseSorting();

        // TODO: Resolver for Members via TeamUsers
    }
}