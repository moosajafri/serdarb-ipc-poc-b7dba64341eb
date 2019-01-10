using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IPC.Database
{
    public class PageList<T> where T : BaseEntity
    {
        //todo: think about the fields

        public long LastId { get; set; }
        public long FirstId { get; set; }

        public bool IsHavingPrevious { get; set; }
        public bool IsHavingNext { get; set; }

        public long Total { get; set; }

        public List<T> Items { get; set; }

        public PageList()
        {
            Items = new List<T>();
        }
    }

    public class BaseEntity
    {
        //todo: think about the fields

        public int Id { get; set; }
        public string Guid { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }

    public class BaseEntityRevision
    {
        //todo: think about the fields

        public int RevisionId { get; set; }
        public int Id { get; set; }
    }

    public interface IBaseRepository<T, R> where T : BaseEntity
                                           where R : BaseEntityRevision
    {
        string SchemaName { get; set; }

        /// <summary>
        /// throws exception if updates more than one row for table
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="currentUserId"></param>
        /// <returns>primary key</returns>
        Task<int> Insert(T entity, int currentUserId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="currentUserId"></param>
        /// <returns>inserted row count</returns>
        Task<int> InsertBulk(IEnumerable<T> entities, int currentUserId);

        /// <summary>
        /// updates via primary key
        /// inserts revision for only changed columns
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="currentUserId"></param>
        Task<bool> Update(T entity, int currentUserId);

        Task<T> SelectByPrimaryKey(long id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="isAscending"></param>
        /// <returns></returns>
        Task<List<T>> SelectAll(int skip = 0, int take = 1000, bool isAscending = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastId"></param>
        /// <param name="take"></param>
        /// <param name="isAscending"></param>
        /// <returns></returns>
        Task<T> SelectAfter(int lastId, int take = 100, bool isAscending = true);

        /// <summary>
        /// saves the last revision as deleted does not deletes the revision!
        /// </summary>
        /// <param name="id"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        Task<bool> Delete(int id, int currentUserId);
        Task<bool> UndoDelete(int id, int currentUserId);

        Task<List<R>> SelectRevisions(int id);

        /// <summary>
        /// if null works with primary key
        /// </summary>
        
        Task<bool> Any(Expression<Func<T, bool>> where = null);
        Task<long> Count(Expression<Func<T, bool>> where = null);
        Task<long> Max(Expression<Func<T, int>> column = null, Expression<Func<T, bool>> where = null);
        Task<long> Min(Expression<Func<T, int>> column  = null, Expression<Func<T, bool>> where = null);
        Task<long> Sum(Expression<Func<T, long>> column  = null, Expression<Func<T, bool>> where = null);
    }
}