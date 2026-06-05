using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using UserService.Application.Abstractions;
using UserService.Application.Dtos;
using UserService.Application.Events;
using UserService.Application.Options;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Tests
{
    public class UserAppServiceTests
    {
        private Mock<IUserRepository> m_userRepo;
        private Mock<IBalanceHistoryRepository> m_historyRepo;
        private Mock<IUnitOfWork> m_uow;
        private Mock<IOutboxWriter> m_outbox;
        private IOptions<AppSettings> m_settings;

        private UserAppService m_service;

        [SetUp]
        public void Setup()
        {
            this.m_userRepo = new Mock<IUserRepository>();
            this.m_historyRepo = new Mock<IBalanceHistoryRepository>();
            this.m_uow = new Mock<IUnitOfWork>();
            this.m_outbox = new Mock<IOutboxWriter>();

            this.m_settings = Options.Create(new AppSettings {
                MaxBalance = 1000m
            });

            this.m_service = new UserAppService(
                this.m_userRepo.Object,
                this.m_historyRepo.Object,
                this.m_uow.Object,
                this.m_settings,
                this.m_outbox.Object);
        }

        [Test]
        public async Task CreateUser_Should_Save_User_And_Return_It()
        {
            UserDto dto = new UserDto("Maxim", new DateOnly(1972, 7, 21), "Vidnoe");

            User? captured = null;

            this.m_userRepo
                .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((u, _) => captured = u)
                .Returns(Task.CompletedTask);

            this.m_uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            User result = await this.m_service.CreateUserAsync(dto, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FullName, Is.EqualTo("Maxim"));
            Assert.That(result.Balance, Is.EqualTo(0m));

            this.m_userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            this.m_uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateBalance_Should_Update_User_And_Call_Outbox()
        {
            User user = User.Create("Maxim", new DateOnly(1972, 7, 21), "Vidnoe");

            this.m_userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            this.m_uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            UpdateBalanceDto dto = new UpdateBalanceDto(user.Id, 100);

            await this.m_service.UpdateBalanceAsync(dto, CancellationToken.None);

            Assert.That(user.Balance, Is.EqualTo(100m));

            this.m_outbox.Verify(o => o.WriteAsync(
                    It.IsAny<UserBalanceChangedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void UpdateBalance_Should_Throw_When_User_Not_Found()
        {
            UpdateBalanceDto dto = new UpdateBalanceDto(Guid.NewGuid(), 100);

            this.m_userRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            Assert.ThrowsAsync<DomainException>(async () =>
                await this.m_service.UpdateBalanceAsync(dto, CancellationToken.None));
        }

        [Test]
        public async Task UpdateBalance_Should_Throw_When_Exceeds_MaxBalance()
        {
            User user = User.Create("Maxim", new DateOnly(1972, 7, 21), "Vidnoe");

            this.m_userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            UpdateBalanceDto dto = new UpdateBalanceDto(user.Id, 10_000);

            Assert.ThrowsAsync<DomainException>(async () =>
                await this.m_service.UpdateBalanceAsync(dto, CancellationToken.None));
        }

        [Test]
        public async Task Bulk_Update_Should_Update_All_Users()
        {
            User user1 = User.Create("Maxim", new DateOnly(1972, 7, 21), "Vidnoe");
            User user2 = User.Create("Natalia", new DateOnly(1979, 5, 17), "Vidnoe");

            UpdateBalanceDto[] dtos = new[]
            {
                new UpdateBalanceDto(user1.Id, 10),
                new UpdateBalanceDto(user2.Id, 20)
            };

            this.m_userRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { user1, user2 });

            this.m_uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await this.m_service.UpdateBalanceBulkAsync(dtos, CancellationToken.None);

            Assert.That(user1.Balance, Is.EqualTo(10m));
            Assert.That(user2.Balance, Is.EqualTo(20m));

            this.m_outbox.Verify(x => x.WriteAsync(
                    It.IsAny<UserBalanceChangedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task GetRecentHistory_Should_Map_User_Names()
        {
            User user = User.Create("Maxim", new DateOnly(1972, 7, 21), "Vidnoe");

            List<BalanceHistory> history = new List<BalanceHistory>
            {
                BalanceHistory.Create(user.Id, 100, 100)
            };

            this.m_historyRepo.Setup(r => r.GetRecentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(history);

            this.m_userRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { user });

            IReadOnlyList<BalanceHistoryDto> result = await this.m_service.GetRecentBalanceHistoryAsync(CancellationToken.None);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].UserFullName, Is.EqualTo("Maxim"));
        }
    }
}
