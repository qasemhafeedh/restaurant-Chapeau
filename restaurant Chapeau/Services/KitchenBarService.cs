using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    public class KitchenBarService : IKitchenBarService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<KitchenBarService> _logger; // Fixed logger type

        // ✅ ADD THIS CONSTRUCTOR
        public KitchenBarService(IOrderRepository orderRepository, ILogger<KitchenBarService> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Order GetOrderById(int orderId)
        {
            try
            {
                List<Order> allOrders = _orderRepository.GetAllOrders();
                return allOrders.FirstOrDefault(o => o.Id == orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID: {OrderId}", orderId);
                throw;
            }
        }

        public List<Order> GetRunningOrders()
        {
            List<Order> runningOrders = _orderRepository.GetAllOrders();
            return runningOrders.Where(o => o.Status == OrderStatus.Running).ToList();
        }

        public List<Order> GetFinishedOrders()
        {
            try
            {
                List<Order> finishedOrders = _orderRepository.GetAllOrders();
                var today = DateTime.Today;
                return finishedOrders
                    .Where(o => o.Status == OrderStatus.Finished && o.OrderTime.Date == today)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting finished orders");
                throw;
            }
        }

        public void UpdateOrderItemStatus(int orderItemId, ItemStatus newStatus)
        {
            try
            {
                _orderRepository.UpdateOrderItemStatus(orderItemId, newStatus);
                _logger.LogInformation("Updated order item {ItemId} to status {Status}", orderItemId, newStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order item status: {ItemId} to {Status}", orderItemId, newStatus);
                throw;
            }
        }

        public void UpdeteOrderStatus(Order order, OrderStatus newStatus)
        {
            try
            {
                order.Status = newStatus;
                _logger.LogInformation("Updated order {OrderId} to status {Status}", order.Id, newStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId} to {Status}", order.Id, newStatus);
                throw;
            }
        }

        public void UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus)
        {
            try
            {
                _logger.LogInformation($"Updating course status for Order: {orderId}, Course: {courseType}, New Status: {newStatus}");

                // Get the order with all items
                var order = GetOrderById(orderId);
                if (order == null)
                {
                    _logger.LogError($"Order {orderId} not found");
                    throw new ArgumentException($"Order {orderId} not found");
                }

                // Find all items for the specified course type
                var courseItems = order.Items.Where(item => item.courseType == courseType).ToList();

                if (!courseItems.Any())
                {
                    _logger.LogWarning($"No items found for course {courseType} in order {orderId}");
                    return;
                }

                // Update each item in the course
                foreach (var item in courseItems)
                {
                    if (IsValidStatusTransition(item.itemStatus, newStatus))
                    {
                        _orderRepository.UpdateOrderItemStatus(item.Id, newStatus);
                        _logger.LogInformation($"Updated item {item.Id} ({item.Name}) to {newStatus}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course status: Order {OrderId}, Course {CourseType}", orderId, courseType);
                throw;
            }
        }

        private bool IsValidStatusTransition(ItemStatus currentStatus, ItemStatus newStatus)
        {
            // Define valid transitions according to requirements
            return (currentStatus, newStatus) switch
            {
                (ItemStatus.Pending, ItemStatus.Preparing) => true,
                (ItemStatus.Preparing, ItemStatus.Ready) => true,
                // Allow any status to be set individually (for flexibility)
                _ when currentStatus != newStatus => true,
                _ => false
            };
        }

        public List<Order> FilterOrdersByTarget(List<Order> orders, Order.RoutingTarget target)
        {
            try
            {
                return orders
                    .Select(order =>
                    {
                        var targetItems = order.Items
                            .Where(item => item.target == target)
                            .ToList();

                        return new Order(
                            order.Id,
                            order.TableNumber,
                            order.OrderTime,
                            targetItems,
                            order.Comment,
                            order.Status
                        );
                    })
                    .Where(order => order.Items.Any())
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering orders by target: {Target}", target);
                throw;
            }
        }
    }
}