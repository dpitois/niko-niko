using System.Linq;
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

        descriptor.Field("members")
            .Description("The members of the team.")
            .Resolve(context =>
            {
                var team = context.Parent<Team>();
                var members = team.TeamUsers.Select(tu => tu.User).ToList();

                // Ensure Admin is included in members list for the Dashboard grid
                if (team.Admin != null && !members.Any(m => m.Id == team.AdminId))
                {
                    members.Insert(0, team.Admin);
                }

                return members;
            })
            .Type<NonNullType<ListType<NonNullType<UserType>>>>();

        descriptor.Field(t => t.Admin)
            .Type<NonNullType<UserType>>()
            .Description("The administrator of the team.");

        descriptor.Field(t => t.Sprints)
            .Type<NonNullType<ListType<NonNullType<SprintType>>>>()
            .UseFiltering()
            .UseSorting();

        descriptor.Field("members")
            .Resolve(context => context.Parent<Team>().TeamUsers.Select(tu => tu.User))
            .Type<NonNullType<ListType<NonNullType<UserType>>>>();
    }
}