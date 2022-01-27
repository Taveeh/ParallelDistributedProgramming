import repository.InventoryRepository;
import repository.ProductLockInventoryRepository;
import service.InventoryService;

public class Main {
    public static void main(String[] args) {
        InventoryRepository inventory = new ProductLockInventoryRepository();

        InventoryService inventoryService = new InventoryService(inventory);

        System.out.println(inventoryService.runOperations(5, 100, 100000, 100,100));

    }
}
