interface FlagIconProps {
    language: 'en' | 'fr';
    className?: string;
}

function FlagIcon({ language, className = '' }: FlagIconProps) {
    if (language === 'en') {
        // UK Flag (simplified)
        return (
            <svg
                className={className}
                width="20"
                height="15"
                viewBox="0 0 20 15"
                xmlns="http://www.w3.org/2000/svg"
                style={{ display: 'inline-block', verticalAlign: 'middle', marginRight: '4px' }}
            >
                <rect width="20" height="15" fill="#012169" />
                <path d="M 0,0 L 20,15 M 20,0 L 0,15" stroke="#fff" strokeWidth="3" />
                <path d="M 0,0 L 20,15 M 20,0 L 0,15" stroke="#C8102E" strokeWidth="2" />
                <path d="M 10,0 L 10,15 M 0,7.5 L 20,7.5" stroke="#fff" strokeWidth="5" />
                <path d="M 10,0 L 10,15 M 0,7.5 L 20,7.5" stroke="#C8102E" strokeWidth="3" />
            </svg>
        );
    } else {
        // French Flag
        return (
            <svg
                className={className}
                width="20"
                height="15"
                viewBox="0 0 20 15"
                xmlns="http://www.w3.org/2000/svg"
                style={{ display: 'inline-block', verticalAlign: 'middle', marginRight: '4px' }}
            >
                <rect width="6.67" height="15" x="0" fill="#002395" />
                <rect width="6.67" height="15" x="6.67" fill="#FFFFFF" />
                <rect width="6.67" height="15" x="13.33" fill="#ED2939" />
            </svg>
        );
    }
}

export default FlagIcon;
