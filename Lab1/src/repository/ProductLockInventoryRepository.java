package repository;

import model.Bill;
import model.Product;

import java.util.*;

public class ProductLockInventoryRepository implements InventoryRepository {
    private Map<Product, Integer> initialStock;
    private Map<Product, Integer> currentStock;
    private int income;
    private final List<Bill> processedBills;

    public ProductLockInventoryRepository(){
        this.initialStock = new HashMap<>();
        this.currentStock = new HashMap<>();
        this.income = 0;
        this.processedBills = new ArrayList<>();
    }

    @Override
    public synchronized void resetStock(List<AbstractMap.SimpleEntry<Product, Integer>> productList) {
        this.initialStock = new HashMap<>();
        this.currentStock = new HashMap<>();

        productList.forEach(product -> {
            initialStock.put(product.getKey(), product.getValue());
            currentStock.put(product.getKey(), product.getValue());
        });
    }

    @Override
    public synchronized void checkInventory() {
        System.out.println("Checking inventory");
        int expectedSum = this.processedBills.stream()
                .map(Bill::getTotalSum)
                .reduce(0, Integer::sum);

        if (expectedSum != this.income) {
            System.out.println("INVENTORY CHECK FAILED - INCOME DOES NOT CORRESPOND WITH SALES OPERATIONS");
        }

    }

    @Override
    public void processBill(Bill bill) {
        // process bill & lock only modified products
        for (AbstractMap.SimpleEntry<Product, Integer> billProduct: bill.getProductList()) {
            currentStock.computeIfPresent(billProduct.getKey(), (inventoryProduct, quantity) -> {
                    if (billProduct.getValue().compareTo(quantity) <= 0) {
                        return quantity - billProduct.getValue();
                    } else {
                        System.out.println("Insufficient stock");
                        return quantity;
                    }
            });
        }

        synchronized (this) {
            this.income += bill.getTotalSum();
            this.processedBills.add(bill);
        }
    }
}
