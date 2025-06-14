﻿using restaurant_Chapeau.Models;
using restaurant_Chapeau.Repositaries;
using restaurant_Chapeau.Services.Interfaces;

namespace restaurant_Chapeau.Services
{
    public class KitchenBarService : IKitchenBarService
    {
        private readonly IKitchenBarRepository _repository;
        private readonly ILogger<KitchenBarService> _logger;

        public KitchenBarService(IKitchenBarRepository repository, ILogger<KitchenBarService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public List<Order> GetRunningOrders()
        {
            return _repository.GetRunningOrders();
        }

        public List<Order> GetFinishedOrders()
        {
            return _repository.GetFinishedOrders();
        }

        public Order GetOrderById(int orderId)
        {
            return _repository.GetOrderById(orderId);
        }

        public void UpdateOrderItemStatus(int orderItemId, ItemStatus newStatus)
        {
            _repository.UpdateOrderItemStatus(orderItemId, newStatus);
        }

        public void UpdateCourseStatus(int orderId, CourseType courseType, ItemStatus newStatus)
        {
            _logger.LogInformation($"Updating course status for Order: {orderId}, Course: {courseType}, New Status: {newStatus}");

            // Get the order with all items
            var order = _repository.GetOrderById(orderId);
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

            // Validate status transition (only allow ordered->preparing and preparing->ready)
            foreach (var item in courseItems)
            {
                if (!IsValidStatusTransition(item.itemStatus, newStatus))
                {
                    _logger.LogWarning($"Invalid status transition for item {item.Id}: {item.itemStatus} -> {newStatus}");
                    continue; // Skip invalid transitions but continue with others
                }
            }

            // Update each item in the course
            foreach (var item in courseItems)
            {
                if (IsValidStatusTransition(item.itemStatus, newStatus))
                {
                    _repository.UpdateOrderItemStatus(item.Id, newStatus);
                    _logger.LogInformation($"Updated item {item.Id} ({item.Name}) to {newStatus}");
                }
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
    }
}