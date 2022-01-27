package model.runnables;

import model.Bill;
import repository.InventoryRepository;

import java.util.List;

public class OperationRunnable implements Runnable {

    private final InventoryRepository inventory;
    private final List<Bill> billList;

    public OperationRunnable(InventoryRepository inventory, List<Bill> billList) {
        this.inventory = inventory;
        this.billList = billList;
    }

    @Override
    public void run() {
        try {
            billList.forEach(inventory::processBill);
            inventory.checkInventory();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
