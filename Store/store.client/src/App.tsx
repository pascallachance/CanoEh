import { BrowserRouter as Router, Routes, Route, useNavigate } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import CreateUser from './components/CreateUser';
import ForgotPassword from './components/ForgotPassword';
import Home from './components/Home';
import Cart from './components/Cart';

function AppContent() {
    const navigate = useNavigate();

    const handleLoginSuccess = () => {
        // Navigate to home page after successful login
        console.log('Login successful - user authenticated');
        navigate('/');
    };

    // Show login/register forms
    return (
        <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login onLoginSuccess={handleLoginSuccess} />} />
            <Route path="/CreateUser" element={<CreateUser onCreateSuccess={() => {/* Navigate to login after creation */}} />} />
            <Route path="/RestorePassword" element={<ForgotPassword />} />
            <Route path="/cart" element={<Cart />} />
            <Route path="*" element={<Home />} />
        </Routes>
    );
}

function App() {
    return (
        <Router>
            <AppContent />
        </Router>
    );
}

export default App;