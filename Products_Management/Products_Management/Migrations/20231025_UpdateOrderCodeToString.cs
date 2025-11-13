using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Products_Management.Migrations
{
    public partial class UpdateOrderCodeToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Đầu tiên xóa cột cũ nếu tồn tại
            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "Order");

            // Thêm cột mới với kiểu string
            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "Order",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Thêm các cột PayOS khác nếu chưa có
            migrationBuilder.AddColumn<string>(
                name: "PaymentLink",
                table: "Order",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Order",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Order",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PaymentLink",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Order");
        }
    }
}