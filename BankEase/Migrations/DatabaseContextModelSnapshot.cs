﻿// <auto-generated />
using System;
using BankEase.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BankEase.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BankEase.Models.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(9,2)")
                        .HasColumnName("BALANCE");

                    b.Property<int>("CustomerId")
                        .HasColumnType("int")
                        .HasColumnName("CUSTOMER_ID");

                    b.Property<string>("IBAN")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("IBAN");

                    b.Property<decimal>("Overdraft")
                        .HasColumnType("decimal(7,2)")
                        .HasColumnName("OVERDRAFT");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("IBAN")
                        .IsUnique();

                    b.ToTable("ACCOUNT");
                });

            modelBuilder.Entity("BankEase.Models.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("CITY");

                    b.Property<string>("CustomerNumber")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasColumnName("CUSTOMER_NUMBER");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("FIRST_NAME");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("LAST_NAME");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("STREET");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasColumnName("TITLE");

                    b.Property<short>("ZipCode")
                        .HasColumnType("smallint")
                        .HasColumnName("ZIPCODE");

                    b.HasKey("Id");

                    b.HasIndex("CustomerNumber")
                        .IsUnique();

                    b.ToTable("CUSTOMER");
                });

            modelBuilder.Entity("BankEase.Models.TransactionRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("int")
                        .HasColumnName("ACCOUNT_ID");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(9,2)")
                        .HasColumnName("AMOUNT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("TEXT");

                    b.Property<DateTime>("TransactionTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("TRANSACTION_TIME")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("char(1)")
                        .HasColumnName("TYPE");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("TRANSACTION_RECORD");
                });

            modelBuilder.Entity("BankEase.Models.Account", b =>
                {
                    b.HasOne("BankEase.Models.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("BankEase.Models.TransactionRecord", b =>
                {
                    b.HasOne("BankEase.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });
#pragma warning restore 612, 618
        }
    }
}
