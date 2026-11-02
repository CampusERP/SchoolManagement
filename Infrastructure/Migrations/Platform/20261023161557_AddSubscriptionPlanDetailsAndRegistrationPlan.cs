using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Platform
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlanDetailsAndRegistrationPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SubscriptionPlans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanName",
                table: "SchoolRegistrationRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlanPriceMonthly",
                table: "SchoolRegistrationRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId",
                table: "SchoolRegistrationRequests",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "PlanName",
                table: "SchoolRegistrationRequests");

            migrationBuilder.DropColumn(
                name: "PlanPriceMonthly",
                table: "SchoolRegistrationRequests");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "SchoolRegistrationRequests");
        }
    }
}
