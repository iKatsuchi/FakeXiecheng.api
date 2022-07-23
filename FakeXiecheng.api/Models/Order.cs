using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stateless;

namespace FakeXiecheng.api.Models
{
    public enum OrderStateEnum
    {
        Pending,//订单已生成
        Processing,//支付处理中
        Completed,//交易成功
        Declined,//交易失败
        Cancelled,//订单取消
        Refund//已退款
    }

    public enum OrderStateTriigerEnum
    {
        PlaceOrder,//支付
        Approve,//支付成功
        Reject,//支付失败
        Cancel,//取消
        Return//退货
    }

    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<LineItem> ShoppingCartItems { get; set; }
        public OrderStateEnum State { get; set; }
        public DateTime CreateDateUTC { get; set; }
        public string TransactionMetadata { get; set; }
        StateMachine<OrderStateEnum, OrderStateTriigerEnum> _machine;

        private void StateMachineInit()
        {
            _machine = new StateMachine<OrderStateEnum, OrderStateTriigerEnum>
                (OrderStateEnum.Pending);

            _machine.Configure(OrderStateEnum.Pending)
                .Permit(OrderStateTriigerEnum.PlaceOrder, OrderStateEnum.Processing)
                .Permit(OrderStateTriigerEnum.Cancel, OrderStateEnum.Cancelled);

            _machine.Configure(OrderStateEnum.Processing)
                .Permit(OrderStateTriigerEnum.Approve, OrderStateEnum.Completed)
                .Permit(OrderStateTriigerEnum.Reject, OrderStateEnum.Declined);

            _machine.Configure(OrderStateEnum.Declined)
                .Permit(OrderStateTriigerEnum.PlaceOrder, OrderStateEnum.Processing);

            _machine.Configure(OrderStateEnum.Completed)
                .Permit(OrderStateTriigerEnum.Return, OrderStateEnum.Refund);
        }

    }
}
