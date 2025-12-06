using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doanchuyennganh.Migrations
{
    public partial class ChangeProductIdToLong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Gỡ bỏ khóa chính hiện tại
            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            // Đổi kiểu cột Id từ int sang long (bigint)
            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Products",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            // Thêm lại khóa chính
            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            // Tạo lại foreign key và index
            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa foreign key và index
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails");

            // Gỡ khóa chính hiện tại
            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            // Đổi lại kiểu Id từ long → int
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            // Thêm lại khóa chính cũ
            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");
        }
    }
}
