using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IPC.BusinessServer
{
    public class Student
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' does not refer to a property.", propertyLambda.ToString()));
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' does not refer to a property.", propertyLambda.ToString()));
            }

            if (type != propInfo.ReflectedType
                && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a property that is not from type {1}.", propertyLambda.ToString(), type));
            }

            return propInfo;
        }
    }

    public class StudentRepository
    {
        public List<Student> Students { get; set; }

        public StudentRepository()
        {
            Students = new List<Student>();
        }

        public long Max(Expression<Func<Student, long>> columnInfo)
        {
            var expression = columnInfo.Body as MemberExpression;
            if (expression == null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' does not refer to a property.", columnInfo.ToString()));
            }

            var fieldName = expression.Member.Name;

            //todo:create query

            return 0;
        }

        public bool Where(Expression<Func<Student, bool>> filter)
        {
            var expression = filter.Body as LambdaExpression;
            if (expression == null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' does not refer to a lambda.", filter.ToString()));
            }

            var whereClause = expression.ToString().Replace("\"", "'").Replace("==", "=").Replace("OrElse", "OR").Replace("AndAlso", "AND");

            //todo:create query

            return false;
        }

    }
}