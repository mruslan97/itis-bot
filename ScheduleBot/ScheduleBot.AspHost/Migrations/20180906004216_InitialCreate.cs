using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ScheduleBot.AspHost.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    version = table.Column<long>(nullable: false),
                    created = table.Column<DateTime>(nullable: false),
                    edited = table.Column<DateTime>(nullable: false),
                    GType = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    version = table.Column<long>(nullable: false),
                    created = table.Column<DateTime>(nullable: false),
                    edited = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ChatId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProfileAndGroups",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    version = table.Column<long>(nullable: false),
                    created = table.Column<DateTime>(nullable: false),
                    edited = table.Column<DateTime>(nullable: false),
                    ProfileId = table.Column<long>(nullable: false),
                    GroupId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileAndGroups", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProfileAndGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileAndGroups_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileAndGroups_GroupId",
                table: "ProfileAndGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileAndGroups_ProfileId",
                table: "ProfileAndGroups",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_ChatId",
                table: "Profiles",
                column: "ChatId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileAndGroups");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
