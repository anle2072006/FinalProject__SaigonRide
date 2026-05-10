using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject__SaigonRide.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsForeigner",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsForeigner",
                table: "AspNetUsers");
        }
    }
}
