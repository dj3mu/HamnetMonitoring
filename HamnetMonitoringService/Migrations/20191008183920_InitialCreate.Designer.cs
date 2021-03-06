﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RestService.Database;

#pragma warning disable 1591

namespace HamnetDbRest.Migrations
{
    [DbContext(typeof(QueryResultDatabaseContext))]
    [Migration("20191008183920_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("RestService.Database.MonitoringPerstistence", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("LastMaintenanceEnd");

                    b.Property<DateTime>("LastMaintenanceStart");

                    b.Property<DateTime>("LastQueryEnd");

                    b.Property<DateTime>("LastQueryStart");

                    b.HasKey("Id");

                    b.ToTable("MonitoringStatus");
                });

            modelBuilder.Entity("RestService.Model.Rssi", b =>
                {
                    b.Property<string>("ForeignId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<string>("ForeignCall")
                        .IsRequired();

                    b.Property<string>("Metric")
                        .IsRequired();

                    b.Property<int>("MetricId");

                    b.Property<string>("ParentSubnet")
                        .IsRequired();

                    b.Property<string>("RssiValue")
                        .IsRequired();

                    b.Property<string>("TimeStampString")
                        .IsRequired();

                    b.Property<ulong>("UnixTimeStamp");

                    b.HasKey("ForeignId");

                    b.HasIndex("ForeignId")
                        .IsUnique();

                    b.ToTable("RssiValues");
                });

            modelBuilder.Entity("RestService.Model.RssiFailingQuery", b =>
                {
                    b.Property<string>("Subnet")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AffectedHosts");

                    b.Property<string>("ErrorInfo")
                        .IsRequired();

                    b.Property<DateTime>("TimeStamp");

                    b.HasKey("Subnet");

                    b.ToTable("RssiFailingQueries");
                });
#pragma warning restore 612, 618
        }
    }
}
