using NikoNiko.Core.Models;

namespace NikoNiko.Api.GraphQL.Types;

public class MoodEntryType : ObjectType<MoodEntry>
{
    protected override void Configure(IObjectTypeDescriptor<MoodEntry> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(m => m.Id).Type<NonNullType<IdType>>();
        descriptor.Field(m => m.Date).Type<NonNullType<DateTimeType>>();
        descriptor.Field(m => m.Mood).Type<NonNullType<EnumType<MoodType>>>();
        descriptor.Field(m => m.UserId).Type<NonNullType<IdType>>();
        descriptor.Field(m => m.SprintId).Type<NonNullType<IdType>>();

        descriptor.Field(m => m.User).Type<NonNullType<UserType>>();
        descriptor.Field(m => m.Sprint).Type<NonNullType<SprintType>>();
    }
}