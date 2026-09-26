using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EnergyPlatform.Infrastructure.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analysis_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    current_stage = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    stages = table.Column<string>(type: "jsonb", nullable: false),
                    summary = table.Column<string>(type: "jsonb", nullable: true),
                    error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analysis_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "meters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meter_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    location = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    current_daily_kwh = table.Column<double>(type: "double precision", nullable: false),
                    baseline_daily_kwh = table.Column<double>(type: "double precision", nullable: true),
                    variation_percent = table.Column<double>(type: "double precision", nullable: true),
                    hourly_baseline = table.Column<double[]>(type: "double precision[]", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meters", x => x.id);
                    table.UniqueConstraint("ak_meters_meter_id", x => x.meter_id);
                });

            migrationBuilder.CreateTable(
                name: "anomalies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fingerprint = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    meter_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    analysis_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    detected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_analyzed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    severity = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    confidence = table.Column<double>(type: "double precision", nullable: false),
                    priority = table.Column<double>(type: "double precision", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    recommended_action = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    evidence = table.Column<string>(type: "jsonb", nullable: false),
                    evidence_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    explanation = table.Column<string>(type: "jsonb", nullable: true),
                    explanation_source = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anomalies", x => x.id);
                    table.ForeignKey(
                        name: "fk_anomalies_analysis_runs_analysis_run_id",
                        column: x => x.analysis_run_id,
                        principalTable: "analysis_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_anomalies_meters_meter_id",
                        column: x => x.meter_id,
                        principalTable: "meters",
                        principalColumn: "meter_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meter_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_events_meters_meter_id",
                        column: x => x.meter_id,
                        principalTable: "meters",
                        principalColumn: "meter_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "readings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    meter_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    consumption_kwh = table.Column<double>(type: "double precision", nullable: false),
                    voltage_v = table.Column<double>(type: "double precision", nullable: false),
                    current_a = table.Column<double>(type: "double precision", nullable: false),
                    power_factor = table.Column<double>(type: "double precision", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_readings", x => x.id);
                    table.ForeignKey(
                        name: "fk_readings_meters_meter_id",
                        column: x => x.meter_id,
                        principalTable: "meters",
                        principalColumn: "meter_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "anomaly_status_changes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anomaly_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    to_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    changed_by = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anomaly_status_changes", x => x.id);
                    table.ForeignKey(
                        name: "fk_anomaly_status_changes_anomalies_anomaly_id",
                        column: x => x.anomaly_id,
                        principalTable: "anomalies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_analysis_runs_status_requested_at",
                table: "analysis_runs",
                columns: new[] { "status", "requested_at" });

            migrationBuilder.CreateIndex(
                name: "ix_anomalies_analysis_run_id_priority",
                table: "anomalies",
                columns: new[] { "analysis_run_id", "priority" });

            migrationBuilder.CreateIndex(
                name: "ix_anomalies_fingerprint",
                table: "anomalies",
                column: "fingerprint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_anomalies_meter_id",
                table: "anomalies",
                column: "meter_id");

            migrationBuilder.CreateIndex(
                name: "ix_anomaly_status_changes_anomaly_id",
                table: "anomaly_status_changes",
                column: "anomaly_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_meter_id_timestamp",
                table: "events",
                columns: new[] { "meter_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_meters_status",
                table: "meters",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_readings_meter_id_timestamp",
                table: "readings",
                columns: new[] { "meter_id", "timestamp" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anomaly_status_changes");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "readings");

            migrationBuilder.DropTable(
                name: "anomalies");

            migrationBuilder.DropTable(
                name: "analysis_runs");

            migrationBuilder.DropTable(
                name: "meters");
        }
    }
}
