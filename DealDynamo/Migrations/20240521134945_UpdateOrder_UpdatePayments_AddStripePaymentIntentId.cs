using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealDynamo.Migrations
{
    public partial class UpdateOrder_UpdatePayments_AddStripePaymentIntentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripePaymentId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripePaymentId",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
