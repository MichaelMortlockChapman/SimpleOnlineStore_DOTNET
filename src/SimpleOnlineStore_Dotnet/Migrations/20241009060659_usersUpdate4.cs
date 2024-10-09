using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleOnlineStore_Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class usersUpdate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Admins_creatorId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_Customerid",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Customerid",
                table: "Orders",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_Customerid",
                table: "Orders",
                newName: "IX_Orders_CustomerId");

            migrationBuilder.RenameColumn(
                name: "postalCode",
                table: "Customers",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Customers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "Customers",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "Customers",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Customers",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Customers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "userRoleId",
                table: "AspNetUsers",
                newName: "UserRoleId");

            migrationBuilder.RenameColumn(
                name: "creatorId",
                table: "Admins",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "creation",
                table: "Admins",
                newName: "Creation");

            migrationBuilder.RenameIndex(
                name: "IX_Admins_creatorId",
                table: "Admins",
                newName: "IX_Admins_CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Admins_CreatorId",
                table: "Admins",
                column: "CreatorId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Admins_CreatorId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Orders",
                newName: "Customerid");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                newName: "IX_Orders_Customerid");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "Customers",
                newName: "postalCode");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Customers",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Customers",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Customers",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Customers",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Customers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserRoleId",
                table: "AspNetUsers",
                newName: "userRoleId");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Admins",
                newName: "creatorId");

            migrationBuilder.RenameColumn(
                name: "Creation",
                table: "Admins",
                newName: "creation");

            migrationBuilder.RenameIndex(
                name: "IX_Admins_CreatorId",
                table: "Admins",
                newName: "IX_Admins_creatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Admins_creatorId",
                table: "Admins",
                column: "creatorId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_Customerid",
                table: "Orders",
                column: "Customerid",
                principalTable: "Customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
