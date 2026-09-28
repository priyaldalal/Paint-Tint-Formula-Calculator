using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintTint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaxTintPercent = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    PricePerLitre = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colorants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CostPerMl = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colorants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    HexColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DispenseJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    CanSizeLitres = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    TotalColorantMl = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    TintPercent = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispenseJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispenseJobs_Bases_BaseId",
                        column: x => x.BaseId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispenseJobs_Shades_ShadeId",
                        column: x => x.ShadeId,
                        principalTable: "Shades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormulaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ColorantId = table.Column<int>(type: "INTEGER", nullable: false),
                    MlPerLitre = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulaItems_Bases_BaseId",
                        column: x => x.BaseId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormulaItems_Colorants_ColorantId",
                        column: x => x.ColorantId,
                        principalTable: "Colorants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormulaItems_Shades_ShadeId",
                        column: x => x.ShadeId,
                        principalTable: "Shades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DispenseJobItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DispenseJobId = table.Column<int>(type: "INTEGER", nullable: false),
                    ColorantId = table.Column<int>(type: "INTEGER", nullable: false),
                    DispensedMl = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispenseJobItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispenseJobItems_Colorants_ColorantId",
                        column: x => x.ColorantId,
                        principalTable: "Colorants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispenseJobItems_DispenseJobs_DispenseJobId",
                        column: x => x.DispenseJobId,
                        principalTable: "DispenseJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bases_Name",
                table: "Bases",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colorants_Code",
                table: "Colorants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispenseJobItems_ColorantId",
                table: "DispenseJobItems",
                column: "ColorantId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseJobItems_DispenseJobId",
                table: "DispenseJobItems",
                column: "DispenseJobId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseJobs_BaseId",
                table: "DispenseJobs",
                column: "BaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DispenseJobs_ShadeId",
                table: "DispenseJobs",
                column: "ShadeId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaItems_BaseId",
                table: "FormulaItems",
                column: "BaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaItems_ColorantId",
                table: "FormulaItems",
                column: "ColorantId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaItems_ShadeId_BaseId_ColorantId",
                table: "FormulaItems",
                columns: new[] { "ShadeId", "BaseId", "ColorantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shades_Code",
                table: "Shades",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shades_Name",
                table: "Shades",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispenseJobItems");

            migrationBuilder.DropTable(
                name: "FormulaItems");

            migrationBuilder.DropTable(
                name: "DispenseJobs");

            migrationBuilder.DropTable(
                name: "Colorants");

            migrationBuilder.DropTable(
                name: "Bases");

            migrationBuilder.DropTable(
                name: "Shades");
        }
    }
}
