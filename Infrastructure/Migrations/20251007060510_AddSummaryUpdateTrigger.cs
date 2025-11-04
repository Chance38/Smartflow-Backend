using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlowBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSummaryUpdateTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_summary_on_record_insert()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Upsert logic: Try to update, if not found, insert.
                    IF NEW.""Type"" = 'INCOME' THEN
                        INSERT INTO ""MonthlySummary"" (""Id"", ""UserId"", ""Year"", ""Month"", ""Income"", ""Expense"")
                        VALUES (gen_random_uuid(), NEW.""UserId"", EXTRACT(YEAR FROM NEW.""Date""), EXTRACT(MONTH FROM NEW.""Date""), NEW.""Amount"", 0)
                        ON CONFLICT (""UserId"", ""Year"", ""Month"") DO UPDATE
                        SET ""Income"" = ""MonthlySummary"".""Income"" + NEW.""Amount"";
                    ELSIF NEW.""Type"" = 'EXPENSE' THEN
                        INSERT INTO ""MonthlySummary"" (""Id"", ""UserId"", ""Year"", ""Month"", ""Income"", ""Expense"")
                        VALUES (gen_random_uuid(), NEW.""UserId"", EXTRACT(YEAR FROM NEW.""Date""), EXTRACT(MONTH FROM NEW.""Date""), 0, NEW.""Amount"")
                        ON CONFLICT (""UserId"", ""Year"", ""Month"") DO UPDATE
                        SET ""Expense"" = ""MonthlySummary"".""Expense"" + NEW.""Amount"";
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER summary_update_trigger
                AFTER INSERT ON ""Record""
                FOR EACH ROW
                EXECUTE FUNCTION update_summary_on_record_insert();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS summary_update_trigger ON ""Record"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS update_summary_on_record_insert();");
        }
    }
}
