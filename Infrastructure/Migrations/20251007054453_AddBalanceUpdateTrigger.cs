using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlowBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceUpdateTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_balance_on_record_insert()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Check if the record type is EXPENSE
                    IF NEW.""Type"" = 'EXPENSE' THEN
                        UPDATE ""Balance""
                        SET ""Amount"" = ""Amount"" - NEW.""Amount""
                        WHERE ""UserId"" = NEW.""UserId"";
                    -- Check if the record type is INCOME
                    ELSIF NEW.""Type"" = 'INCOME' THEN
                        UPDATE ""Balance""
                        SET ""Amount"" = ""Amount"" + NEW.""Amount""
                        WHERE ""UserId"" = NEW.""UserId"";
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER balance_update_trigger
                AFTER INSERT ON ""Record""
                FOR EACH ROW
                EXECUTE FUNCTION update_balance_on_record_insert();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS balance_update_trigger ON ""Record"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS update_balance_on_record_insert();");
        }
    }
}
