using System.Linq.Expressions;
using System.Reflection;

namespace GardenLog.SharedKernel
{
    // This can be modified to BaseEntity<TId> to support multiple key types (e.g. Guid)
    public abstract class BaseEntity
    {
        public string Id { get; set; } = string.Empty;

        public List<BaseDomainEvent> DomainEvents = new();
        public bool IsModified { get; private set; }


        public void Set<T>(Expression<Func<T>> prop, T value)
        {
            var expr = (MemberExpression)prop.Body;
            var mem = (PropertyInfo)expr.Member;

            var currentValue = mem.GetValue(this);

            if (currentValue == null && value == null)
                return;

            if ((currentValue == null && value != null)
                || (currentValue != null && value == null)
                || currentValue != null &&!currentValue.Equals(value))
            {
                mem.SetValue(this, value);
                IsModified = true;
                AddDomainEvent(mem.Name);
            }
        }


        public void SetCollection<T>(Expression<Func<List<T>>> prop, List<T> newList, string propertyName)
        {
            bool changed = false;
            var expr = (MemberExpression)prop.Body;

            var func = prop.Compile();

            var existingList = (List<T>)func();

            var elementsToRemove = existingList.Where(t => !newList.Contains(t));
            if (elementsToRemove.Any())
            {
                //logic to do something in case if tags are in use
                existingList.RemoveAll(t => elementsToRemove.Contains(t));
                changed = true;
            }

            changed = newList.RemoveAll(t => existingList.Contains(t)) > 0 ;
            

            existingList.AddRange(newList);

            if (newList.Count > 0)
            {
                changed = true;
            }

            if (changed)
            {
                IsModified = true;
                AddDomainEvent(propertyName);
            }
        }

        protected abstract void AddDomainEvent(string attributeName);
    }
}
