import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './Cart.css';

interface CartItem {
    id: number;
    name: string;
    price: number;
    quantity: number;
}

function Cart() {
    const getInitialLanguage = (): string => {
        if (typeof navigator !== 'undefined' && navigator.language) {
            const lang = navigator.language.toLowerCase();
            return lang.startsWith('fr') ? 'fr' : 'en';
        }
        return 'en';
    };

    const navigate = useNavigate();
    const [language] = useState<string>(getInitialLanguage); // TODO: Get from context/parent
    const [cartItems] = useState<CartItem[]>([]); // TODO: Implement cart state management

    const getText = (en: string, fr: string) => language === 'fr' ? fr : en;

    const handleContinueShopping = () => {
        navigate('/');
    };

    const handleCheckout = () => {
        // TODO: Implement checkout functionality
        console.log('Proceed to checkout');
    };

    const getTotalPrice = () => {
        return cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    };

    return (
        <div className="cart-container">
            <div className="cart-header">
                <h1>{getText("Shopping Cart", "Panier d'achat")}</h1>
            </div>

            {cartItems.length === 0 ? (
                <div className="cart-empty">
                    <h2>{getText("Your cart is empty", "Votre panier est vide")}</h2>
                    <p>{getText("Start shopping to add items to your cart", "Commencez à magasiner pour ajouter des articles à votre panier")}</p>
                    <button onClick={handleContinueShopping} className="continue-shopping-btn">
                        {getText("Continue Shopping", "Continuer vos achats")}
                    </button>
                </div>
            ) : (
                <div className="cart-content">
                    <div className="cart-items">
                        {cartItems.map((item) => (
                            <div key={item.id} className="cart-item">
                                <div className="item-details">
                                    <h3>{item.name}</h3>
                                    <p>{getText("Price:", "Prix:")} ${item.price.toFixed(2)}</p>
                                    <p>{getText("Quantity:", "Quantité:")} {item.quantity}</p>
                                </div>
                            </div>
                        ))}
                    </div>
                    <div className="cart-summary">
                        <h2>{getText("Order Summary", "Résumé de la commande")}</h2>
                        <div className="summary-row">
                            <span>{getText("Subtotal:", "Sous-total:")}</span>
                            <span>${getTotalPrice().toFixed(2)}</span>
                        </div>
                        <button className="checkout-btn" onClick={handleCheckout}>
                            {getText("Proceed to Checkout", "Passer à la caisse")}
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Cart;
