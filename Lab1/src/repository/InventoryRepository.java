package repository;

import model.Bill;
import model.Product;

import java.util.AbstractMap;
import java.util.List;

public interface InventoryRepository {
    void resetStock(List<AbstractMap.SimpleEntry<Product, Integer>> productList);
    void checkInventory();
    void processBill(Bill bill);
}