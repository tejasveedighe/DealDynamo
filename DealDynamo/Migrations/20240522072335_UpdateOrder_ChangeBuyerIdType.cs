using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealDynamo.Migrations
{
    public partial class UpdateOrder_ChangeBuyerIdType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Orders_AspNetUsers_BuyerId1",
            //    table: "Orders");

            //migrationBuilder.DropIndex(
            //    name: "IX_Orders_BuyerId1",
            //    table: "Orders");

            //migrationBuilder.DropColumn(
            //    name: "BuyerId1",
            //    table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "BuyerId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BuyerId",
                table: "Orders",
                column: "BuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_BuyerId",
                table: "Orders",
                column: "BuyerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_BuyerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BuyerId",
                table: "Orders");

            migrationBuilder.AlterColumn<Guid>(
                name: "BuyerId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "BuyerId1",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BuyerId1",
                table: "Orders",
                column: "BuyerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_BuyerId1",
                table: "Orders",
                column: "BuyerId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
