Your current services already cover the essential CRUD and order workflows. However, I see a few useful business operations that are missing, particularly for `MenuService` and `OrderService`.

I'd separate them into essential, useful extensions, and unnecessary for the current project scope.

## 1. CategoryService

Your current methods are sufficient:

| Method         | Purpose                                        |
| -------------- | ---------------------------------------------- |
| `CreateAsync`  | Create a category                              |
| `GetAllAsync`  | Retrieve categories with pagination and search |
| `GetByIdAsync` | Retrieve a category                            |
| `UpdateAsync`  | Update category details                        |
| `DeleteAsync`  | Delete category if it has no menu items        |

Recommendation: No additional method needed.

You could add duplicate category-name checking, but I'd consider it an additional business rule rather than an essential operation.

## 2. MenuService

Your current methods:

- `CreateAsync`
- `GetAllAsync`
- `GetByIdAsync`
- `UpdateAsync`
- `DeleteAsync`

I'd add two operations:

| Method                    | Purpose                                 | Priority |
| ------------------------- | --------------------------------------- | -------- |
| `UpdateAvailabilityAsync` | Change whether a menu item is available | High     |
| `GetAvailableAsync`       | Retrieve only available menu items      | Medium   |

### Why `UpdateAvailabilityAsync`?

Currently, your `CreateMenuItemDto` and `UpdateMenuItemDto` deliberately don't expose `IsAvailable`.

That means there's no way to change availability through the API.

Rather than allowing ordinary menu updates to modify it, create a dedicated operation:

```
Task<ApiResponseDto<MenuItemResponseDto>> UpdateAvailabilityAsync(
    int id,
    bool isAvailable);
```

This separates menu information updates from availability management.

## 3. OrderService

Your current methods:

- `CreateAsync`
- `GetAllAsync`
- `GetByIdAsync`
- `UpdateCustomerNameAsync`
- `DeleteAsync`
- `AddItemAsync`
- `RemoveItemAsync`
- `CheckoutAsync`

I'd consider these additional operations:

| Method                    | Purpose                            | Priority |
| ------------------------- | ---------------------------------- | -------- |
| `UpdateItemQuantityAsync` | Change an existing item's quantity | High     |
| `UpdateStatusAsync`       | Advance order status               | High     |
| `GetByStatusAsync`        | Filter orders by status            | Medium   |
| `CancelAsync`             | Cancel an order                    | Medium   |

### A. UpdateItemQuantityAsync

Currently, customers can add items and remove items, but cannot directly change an existing item's quantity.

For example:

- Current quantity: 2
- Requested quantity: 5
- New quantity: 5 (not 7)

Suggested signature:

```
Task<ApiResponseDto<OrderResponseDto>> UpdateItemQuantityAsync(
    int orderId,
    int orderItemId,
    int quantity);
```

Business rules:

- Order must exist.
- Order must be Pending.
- Order item must exist.
- Quantity must be greater than zero.
- Recalculate `TotalAmount`.

### B. UpdateStatusAsync

Your checkout currently changes the order status from `Pending` to `Preparing`.

But what happens afterward?

You need a way to advance the order through its lifecycle:

Pending

Checkout

Preparing

Ready

Completed

Cancelled — available only while Pending, under the proposed rules.

Suggested signature:

```
Task<ApiResponseDto<OrderResponseDto>> UpdateStatusAsync(
    int orderId,
    OrderStatus status);
```

However, don't allow arbitrary status changes. Define valid transitions:

- Pending → Preparing (through checkout)
- Preparing → Ready
- Ready → Completed
- Pending → Cancelled

This prevents invalid transitions such as Completed → Preparing.

### C. GetByStatusAsync

Useful for a restaurant staff workflow:

```
Task<ApiResponseDto<List<OrderResponseDto>>> GetByStatusAsync(
    OrderStatus status);
```

For example, staff can retrieve all orders currently being prepared.

### D. CancelAsync

You could implement cancellation as a dedicated method instead of requiring callers to use `UpdateStatusAsync`.

However, if you implement `UpdateStatusAsync` with strict transition rules, a separate cancellation method isn't necessary.

## 4. My recommended final scope

For your current learning project, I would settle on:

| Service         | Methods                                                      |
| --------------- | ------------------------------------------------------------ |
| CategoryService | Existing 5 methods                                           |
| MenuService     | Existing 5 + `UpdateAvailabilityAsync`                       |
| OrderService    | Existing 8 + `UpdateItemQuantityAsync` + `UpdateStatusAsync` |

`GetByStatusAsync` is a good optional addition.

I wouldn't add inventory management, discount calculations, refunds, delivery management, or table reservations yet. Those would expand the domain beyond your current restaurant-ordering scope.

One important architectural point: When adding these methods, also add corresponding repository methods only when the existing repository cannot provide the required data. Keep business rules inside Services, not Repositories.