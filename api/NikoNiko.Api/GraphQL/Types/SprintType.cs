using NikoNiko.Core.Models;

namespace NikoNiko.Api.GraphQL.Types;

public class SprintType : ObjectType<Sprint>
{
    protected override void Configure(IObjectTypeDescriptor<Sprint> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(s => s.Id).Type<NonNullType<IdType>>();
        descriptor.Field(s => s.Name).Type<NonNullType<StringType>>();
        descriptor.Field(s => s.StartDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(s => s.EndDate).Type<NonNullType<DateTimeType>>();
        descriptor.Field(s => s.TeamId).Type<NonNullType<IdType>>();

        descriptor.Field(s => s.Team)
            .Type<NonNullType<TeamType>>();

        descriptor.Field(s => s.MoodEntries)
            .Type<NonNullType<ListType<NonNullType<MoodEntryType>>>>()
            .UseFiltering()
            .UseSorting();
    }
}
