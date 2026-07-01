using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public List<OrderItem> Items { get; } = new();
        public decimal TotalAmount { get; private set; }

        public void AddItem(OrderItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            Items.Add(item);
            TotalAmount += item.Price * item.Quantity;
        }
    }
}
