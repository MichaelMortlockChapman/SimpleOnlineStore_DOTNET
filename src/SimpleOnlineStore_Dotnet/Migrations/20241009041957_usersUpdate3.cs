using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleOnlineStore_Dotnet.Migrations {
    /// <inheritdoc />
    public partial class usersUpdate3 : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<Guid>(
                name: "userRoleId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "userRoleId",
                table: "AspNetUsers");
        }
    }
}
