﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StarterKit.Models;

#nullable disable

namespace StarterKit.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.4");

            modelBuilder.Entity("StarterKit.Models.Admin", b =>
                {
                    b.Property<int>("AdminId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("AdminId");

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("Admin");

                    b.HasData(
                        new
                        {
                            AdminId = 1,
                            Email = "admin1@example.com",
                            Password = "^�H��(qQ��o��)'s`=\rj���*�rB�",
                            UserName = "admin1"
                        },
                        new
                        {
                            AdminId = 2,
                            Email = "admin2@example.com",
                            Password = "\\N@6��G��Ae=j_��a%0�QU��\\",
                            UserName = "admin2"
                        },
                        new
                        {
                            AdminId = 3,
                            Email = "admin3@example.com",
                            Password = "�j\\��f������x�s+2��D�o���",
                            UserName = "admin3"
                        },
                        new
                        {
                            AdminId = 4,
                            Email = "admin4@example.com",
                            Password = "�].��g��Պ��t��?��^�T��`aǳ",
                            UserName = "admin4"
                        },
                        new
                        {
                            AdminId = 5,
                            Email = "admin5@example.com",
                            Password = "E�=���:�-����gd����bF��80]�",
                            UserName = "admin5"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
