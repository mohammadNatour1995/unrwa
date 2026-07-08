using Domain.Records;

namespace Domain.Constants;

public static class UserSet
{
    public static readonly UserDefinition Administrator = new(
        "99191A4B-A468-4718-84A6-82EA3F8A2F16",
        "Administrator",
        "0770675789",
        "Administrator@os.com",
        "AQAAAAIAAYagAAAAEG8KbxoS3v75eM8PSUXgs95kJZfACnwZaxqI1+PNFXpTVv4AcmvynwpkCJ1Z61vZ1w==",
        "14264c52-f3fe-4461-bbd7-d583969a7d0f",
        "143555f7-5e54-404b-bdf6-240d225e87b0",
        "System Administrator",
        new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc).AddTicks(0000));

    public static readonly UserDefinition Admin = new(
        "18F0D165-8BA8-409C-9C21-9954A4BA499E",
        "Admin",
        "0770675789",
        "Admin@os.com",
        "AQAAAAIAAYagAAAAEDhuqN1QBiPjpLeWO5DiJNU42zSY6qK+pVmv+sNAMtZuYcXB/Xe90gW37jswDCaSTw==",
        "b3519d94-bd31-4864-9988-df5b0376bfd1",
        "f09a3ff1-5d0d-4712-a2c7-a9ef7bae437c",
        "System Admin",
        new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc).AddTicks(0000));
}
