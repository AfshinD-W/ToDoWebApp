using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSToDo.Migrations
{
    /// <inheritdoc />
    public partial class InviteTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "Y:\\logo.png",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "Y:\\logo.png");

            migrationBuilder.CreateTable(
                name: "ProjectUsersInvite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    InviteToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(day, 1, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectUsersInvite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectUsersInvite_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectUsersInvite_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUsersInvite_ProjectId",
                table: "ProjectUsersInvite",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUsersInvite_UserId",
                table: "ProjectUsersInvite",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectUsersInvite");

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Y:\\logo.png",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "Y:\\logo.png");
        }
    }
}
