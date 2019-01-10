using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BaseRepoTests
{
    public class BaseRepoTestCases
    {

        #region Aggregate Functions test cases
        [Fact]
        public void Count_Returns_TotalItemsCount()
        {
            //Arrange
            IPC.Database.CustomerRepository repo = new IPC.Database.CustomerRepository();
            //drop and recreate table to avoid duplicate keys
            repo.DropTable();
            repo.CreateTable();
            var customer = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "ddr-123-45f-abc-123-45f-34rf-123",
                Id = 8975,
                Name = "Moosa",
                Phone = "123-456",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 765,
                    Name = "yhyhyhy org"
                }
            };
            var customer2 = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "rdr-123-45f-abc-123-45f-34rf-123",
                Id = 5432,
                Name = "Faisacccldfffddfd",
                Phone = "123-323",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 654,
                    Name = "gghg org"
                }
            };
            repo.Insert(customer, 45342);
            repo.Insert(customer2, 45342);
            //Act
            var returnValue = repo.Count();

            //Assert
            Assert.Equal(2, returnValue);
        }
        [Fact]
        public void Min_Returns_MinVal()
        {
            //Arrange
            IPC.Database.CustomerRepository repo = new IPC.Database.CustomerRepository();
            //drop and recreate table to avoid duplicate keys
            repo.DropTable();
            repo.CreateTable();
            var customer = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "ddr-123-45f-abc-123-45f-34rf-123",
                Id = 8975,
                Name = "Moosa",
                Phone = "123-456",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 765,
                    Name = "yhyhyhy org"
                },
                Tempdigits = 23
            };
            var customer2 = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "rdr-123-45f-abc-123-45f-34rf-123",
                Id = 5432,
                Name = "Faisacccldfffddfd",
                Phone = "123-323",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 654,
                    Name = "gghg org"
                },
                Tempdigits = 27
            };
            repo.Insert(customer, 45342);
            repo.Insert(customer2, 45342);
            //Act
            var returnValue = repo.Min(null, x => x.Tempdigits);

            //Assert
            Assert.Equal(23, returnValue);
        }
        [Fact]
        public void Max_Returns_MaxVal()
        {
            //Arrange
            IPC.Database.CustomerRepository repo = new IPC.Database.CustomerRepository();
            //drop and recreate table to avoid duplicate keys
            repo.DropTable();
            repo.CreateTable();
            var customer = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "ddr-123-45f-abc-123-45f-34rf-123",
                Id = 8975,
                Name = "Moosa",
                Phone = "123-456",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 765,
                    Name = "yhyhyhy org"
                },
                Tempdigits = 23
            };
            var customer2 = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "rdr-123-45f-abc-123-45f-34rf-123",
                Id = 5432,
                Name = "Faisacccldfffddfd",
                Phone = "123-323",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 654,
                    Name = "gghg org"
                },
                Tempdigits = 27
            };
            repo.Insert(customer, 45342);
            repo.Insert(customer2, 45342);
            //Act
            var returnValue = repo.Min(null, x => x.Tempdigits);

            //Assert
            Assert.Equal(27, returnValue);
        }
        #endregion

        #region Select test cases 
        [Fact]
        public void Select_Returns_Entity()
        {
            //Arrange
            IPC.Database.CustomerRepository repo = new IPC.Database.CustomerRepository();
            //drop and recreate table to avoid duplicate keys
            repo.DropTable();
            repo.CreateTable();
            var customer = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "ddr-123-45f-abc-123-45f-34rf-123",
                Id = 8975,
                Name = "Moosa",
                Phone = "123-456",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 765,
                    Name = "yhyhyhy org"
                }
            };
            repo.Insert(customer, 45342);
            //Act
           
            var retObj = Task.Run(async () => await repo.SelectAsync(null)).GetAwaiter().GetResult();
            
            //Assert
            Assert.Equal(8975, retObj.Id);
        }
        [Fact]
        public void Select_Many_Returns_List()
        {
            //Arrange
            IPC.Database.CustomerRepository repo = new IPC.Database.CustomerRepository();
            //drop and recreate table to avoid duplicate keys
            repo.DropTable();
            repo.CreateTable();
            var customer = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "ddr-123-45f-abc-123-45f-34rf-123",
                Id = 8975,
                Name = "Moosa",
                Phone = "123-456",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 765,
                    Name = "yhyhyhy org"
                }
            };
            var customer2 = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "rdr-123-45f-abc-123-45f-34rf-123",
                Id = 5432,
                Name = "Faisacccldfffddfd",
                Phone = "123-323",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 654,
                    Name = "gghg org"
                }
            };
            repo.Insert(customer, 45342);
            repo.Insert(customer2, 45342);
            //Act
            var retList= Task.Run(async () => await repo.SelectManyAsync(null)).GetAwaiter().GetResult();

            //Assert
            Assert.Equal(typeof(List<IPC.Common.Customer>), retList.GetType());
        }
        #endregion

        #region Insert test cases
        [Fact]
        public void Add_Returns_Id()
        {
            //Arrange
            IPC.Database.CustomerRepository repo = new IPC.Database.CustomerRepository();
            //drop and recreate table to avoid duplicate keys
            repo.DropTable();
            repo.CreateTable();
            var customer = new IPC.Common.Customer()
            {
                BornAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = 689595,
                UpdatedBy = 689595,
                Guid = "ddr-123-45f-abc-123-45f-34rf-123",
                Id = 8975,
                Name = "Moosa",
                Phone = "123-456",
                Email = "moosa-yolo@abc.com"
                ,
                Organization = new IPC.Common.Organization()
                {
                    Id = 765,
                    Name = "yhyhyhy org"
                }
            };

            //Act
            var returnValue = repo.Insert(customer, 45342);

            //Assert
            Assert.Equal(returnValue, customer.Id);
        }
        #endregion

    }
}
