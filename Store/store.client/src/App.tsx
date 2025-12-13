import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import CreateUser from './components/CreateUser';
import ForgotPassword from './components/ForgotPassword';

function App() {
    const handleLoginSuccess = () => {
        // Navigate to main content or dashboard after login
        // For now, we could add navigation logic here if needed
        console.log('Login successful - user authenticated');
    };

    // Show login/register forms
    return (
        <Router>
            <Routes>
                <Route path="/login" element={<Login onLoginSuccess={handleLoginSuccess} />} />
                <Route path="/CreateUser" element={<CreateUser onCreateSuccess={() => {/* Navigate to login after creation */}} />} />
                <Route path="/RestorePassword" element={<ForgotPassword />} />
                <Route path="*" element={<Navigate to="/login" replace />} />
            </Routes>
        </Router>
    );
}

export default App;