using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealDynamo.Migrations
{
    public partial class UpdateOrder_AddStripePaymentid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripePaymentId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripePaymentId",
                table: "Orders");
        }
    }
}
