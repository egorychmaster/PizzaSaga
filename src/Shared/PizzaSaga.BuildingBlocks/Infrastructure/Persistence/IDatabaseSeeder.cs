using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PizzaSaga.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Определяет контракт для идемпотентного заполнения базы данных начальными данными.
/// Реализации этого интерфейса должны учитывать, что:
/// - Метод SeedAsync() может вызываться многократно без дублирования данных;
/// - Валидация существования записей должна происходить через AnyAsync(), а не по количеству записей в памяти;
/// - Ошибки инициализации должны приводить к финальной регистрации в логе, но не к тихому пропуску (иначе сервис может стартовать в неконсистентном состоянии).
/// </summary>
/// <typeparam name="TContext">Тип контекста EF Core, реализующего схему базы данных.</typeparam>
public interface IDatabaseSeeder<in TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Выполняет идемпотентное заполнение базы данных начальными данными.
    /// Метод должен:
    /// - Использовать транзакции при необходимости;
    /// - Проверять существующие данные перед добавлением;
    /// - Не вызывать SaveChangesAsync() внутри, если не требуется — лучше оставить это на усмотрение вызывающего кода.
    /// </summary>
    /// <param name="context">Экземпляр контекста базы данных.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию инициализации.</returns>
    Task SeedAsync(TContext context, CancellationToken cancellationToken);
}