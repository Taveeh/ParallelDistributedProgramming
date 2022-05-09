import java.util.ArrayList;
import java.util.concurrent.ExecutionException;

public class Main {
    public static void main(String[] args) throws InterruptedException, ExecutionException {

        Polynomial p = new Polynomial(1000);
        Polynomial q = new Polynomial(1000);

        System.out.println("p:" + p);
        System.out.println("q:" + q);


        System.out.println(classicSequentialMultiplication(p, q));
        System.out.println(classicParallelMultiplication(p, q));

        System.out.println(KaratsubaSequentialMultiplication(p, q));
        System.out.println(KaratsubaParallelMultiplication(p, q));

    }

    private static Polynomial classicSequentialMultiplication(Polynomial p, Polynomial q) {
        Polynomial result = PolynomialHelper.multiplicationSequentialForm(p, q);
        System.out.println("Simple seq: ");
        return result;
    }

    private static Polynomial classicParallelMultiplication(Polynomial p, Polynomial q) throws InterruptedException {
        Polynomial result = PolynomialHelper.multiplicationParallelizedForm(p, q, 2);
        System.out.println("Simple parallel: ");
        return result;
    }

    private static Polynomial KaratsubaSequentialMultiplication(Polynomial p, Polynomial q) {
        Polynomial result = PolynomialHelper.multiplicationKaratsubaSequentialForm(p, q);
        System.out.println("Karatsuba seq: ");
        return result;
    }

    private static Polynomial KaratsubaParallelMultiplication(Polynomial p, Polynomial q) throws ExecutionException,
            InterruptedException {
        Polynomial result = PolynomialHelper.multiplicationKaratsubaParallelizedForm(p, q, 1);
        System.out.println("Karatsuba parallel: ");
        return result;
    }
}
