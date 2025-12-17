import React from 'react';

interface FlagIconProps {
    language: 'en' | 'fr';
    className?: string;
}

const flagStyles: React.CSSProperties = {
    display: 'inline-block',
    verticalAlign: 'middle',
    marginRight: '4px'
};

function FlagIcon({ language, className = '' }: FlagIconProps) {
    const ariaLabel = language === 'en' ? 'English' : 'French';
    
    if (language === 'en') {
        // Canadian Flag (simplified)
        return (
            <svg
                className={className}
                width="20"
                height="15"
                viewBox="0 0 20 15"
                xmlns="http://www.w3.org/2000/svg"
                style={flagStyles}
                role="img"
                aria-label={ariaLabel}
            >
                {/* Red bars on left and right */}
                <rect width="5" height="15" x="0" fill="#FF0000" />
                <rect width="5" height="15" x="15" fill="#FF0000" />
                {/* White center */}
                <rect width="10" height="15" x="5" fill="#FFFFFF" />
                {/* Simplified maple leaf in center */}
                <path d="M 10,4 L 10.5,6 L 12,6.5 L 10.5,7 L 10.8,8.5 L 10,7.5 L 9.2,8.5 L 9.5,7 L 8,6.5 L 9.5,6 Z" fill="#FF0000" />
                <rect width="0.8" height="3" x="9.6" y="8" fill="#FF0000" />
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
                style={flagStyles}
                role="img"
                aria-label={ariaLabel}
            >
                <rect width="6.67" height="15" x="0" fill="#002395" />
                <rect width="6.67" height="15" x="6.67" fill="#FFFFFF" />
                <rect width="6.67" height="15" x="13.33" fill="#ED2939" />
            </svg>
        );
    }
}

export default FlagIcon;
