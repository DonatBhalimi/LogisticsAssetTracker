using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsAssetTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMovementApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovementApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedCondition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedSourceType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestReason = table.Column<string>(type: "text", nullable: false),
                    DecisionNote = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovementApprovals_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementApprovals_Locations_RequestedLocationId",
                        column: x => x.RequestedLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementApprovals_Users_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementApprovals_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovementApprovals_AssetId",
                table: "MovementApprovals",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementApprovals_DecidedByUserId",
                table: "MovementApprovals",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementApprovals_RequestedByUserId",
                table: "MovementApprovals",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementApprovals_RequestedLocationId",
                table: "MovementApprovals",
                column: "RequestedLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovementApprovals");
        }
    }
}
