// AutoEdge Sidebar JavaScript
// Handles sidebar toggle, submenu expansion, and responsive behavior

document.addEventListener('DOMContentLoaded', function() {
    // Initialize sidebar functionality
    initSidebar();
    initSubmenus();
    initResponsive();
    initActiveStates();
});

// ===== SIDEBAR TOGGLE FUNCTIONALITY =====
function initSidebar() {
    const sidebarToggle = document.querySelector('.sidebar-toggle');
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('show');
        });

        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', function(e) {
            if (window.innerWidth <= 768) {
                if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                    sidebar.classList.remove('show');
                }
            }
        });

        // Handle window resize
        window.addEventListener('resize', function() {
            if (window.innerWidth > 768) {
                sidebar.classList.remove('show');
            }
        });
    }
}

// ===== SUBMENU FUNCTIONALITY =====
function initSubmenus() {
    const submenuItems = document.querySelectorAll('.has-submenu > .nav-link');

    submenuItems.forEach(function(item) {
        item.addEventListener('click', function(e) {
            e.preventDefault();
            
            const parentItem = this.parentElement;
            const submenu = parentItem.querySelector('.submenu');
            
            // Close other open submenus
            document.querySelectorAll('.has-submenu').forEach(function(otherItem) {
                if (otherItem !== parentItem) {
                    otherItem.classList.remove('open');
                    const otherSubmenu = otherItem.querySelector('.submenu');
                    if (otherSubmenu) {
                        otherSubmenu.classList.remove('show');
                    }
                }
            });

            // Toggle current submenu
            parentItem.classList.toggle('open');
            if (submenu) {
                submenu.classList.toggle('show');
            }
        });
    });
}

// ===== RESPONSIVE BEHAVIOR =====
function initResponsive() {
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');

    function handleResize() {
        if (window.innerWidth <= 768) {
            // Mobile: sidebar should be hidden by default
            if (sidebar) {
                sidebar.classList.remove('show');
            }
        } else {
            // Desktop: ensure sidebar is visible
            if (sidebar) {
                sidebar.classList.remove('show');
            }
        }
    }

    window.addEventListener('resize', handleResize);
    handleResize(); // Call once on load
}

// ===== ACTIVE STATES =====
function initActiveStates() {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.sidebar-nav .nav-link');

    navLinks.forEach(function(link) {
        const href = link.getAttribute('href');
        if (href && currentPath.includes(href.replace(/^\//, ''))) {
            link.classList.add('active');
            
            // If it's in a submenu, open the parent
            const submenuItem = link.closest('.submenu');
            if (submenuItem) {
                const parentItem = submenuItem.closest('.has-submenu');
                if (parentItem) {
                    parentItem.classList.add('open');
                    submenuItem.classList.add('show');
                }
            }
        }
    });
}

// ===== UTILITY FUNCTIONS =====
window.AutoEdgeSidebar = {
    // Toggle sidebar
    toggle: function() {
        const sidebar = document.querySelector('.sidebar');
        if (sidebar) {
            sidebar.classList.toggle('show');
        }
    },

    // Close sidebar
    close: function() {
        const sidebar = document.querySelector('.sidebar');
        if (sidebar) {
            sidebar.classList.remove('show');
        }
    },

    // Open sidebar
    open: function() {
        const sidebar = document.querySelector('.sidebar');
        if (sidebar) {
            sidebar.classList.add('show');
        }
    },

    // Update active state for a specific link
    setActive: function(linkSelector) {
        // Remove active from all links
        document.querySelectorAll('.sidebar-nav .nav-link').forEach(function(link) {
            link.classList.remove('active');
        });

        // Add active to specified link
        const targetLink = document.querySelector(linkSelector);
        if (targetLink) {
            targetLink.classList.add('active');
        }
    }
};

// ===== SMOOTH SCROLLING =====
document.querySelectorAll('a[href^="#"]').forEach(function(anchor) {
    anchor.addEventListener('click', function(e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// ===== LOADING STATES =====
function showLoading(element) {
    if (element) {
        element.classList.add('loading');
        const originalText = element.innerHTML;
        element.innerHTML = '<span class="spinner"></span> Loading...';
        return function() {
            element.classList.remove('loading');
            element.innerHTML = originalText;
        };
    }
}

// ===== TOAST NOTIFICATIONS =====
function showToast(message, type = 'info') {
    const toastContainer = document.querySelector('.toast-container') || createToastContainer();
    
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
    
    // Remove toast element after it's hidden
    toast.addEventListener('hidden.bs.toast', function() {
        toast.remove();
    });
}

function createToastContainer() {
    const container = document.createElement('div');
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1055';
    document.body.appendChild(container);
    return container;
}

// ===== FORM ENHANCEMENTS =====
document.querySelectorAll('form').forEach(function(form) {
    form.addEventListener('submit', function(e) {
        const submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) {
            showLoading(submitBtn);
        }
    });
});

// ===== CARD HOVER EFFECTS =====
document.querySelectorAll('.card').forEach(function(card) {
    card.addEventListener('mouseenter', function() {
        this.style.transform = 'translateY(-2px)';
    });
    
    card.addEventListener('mouseleave', function() {
        this.style.transform = 'translateY(0)';
    });
});

// ===== TABLE ROW HOVER =====
document.querySelectorAll('.table tbody tr').forEach(function(row) {
    row.addEventListener('mouseenter', function() {
        this.style.backgroundColor = '#f8f9fa';
    });
    
    row.addEventListener('mouseleave', function() {
        this.style.backgroundColor = '';
    });
});

// ===== SEARCH FUNCTIONALITY =====
function initSearch() {
    const searchInputs = document.querySelectorAll('input[type="search"], input[name*="search"]');
    
    searchInputs.forEach(function(input) {
        let searchTimeout;
        
        input.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function() {
                // Trigger search after 300ms of no typing
                const form = input.closest('form');
                if (form) {
                    form.submit();
                }
            }, 300);
        });
    });
}

// Initialize search functionality
initSearch();

// ===== EXPORT FUNCTIONS FOR GLOBAL USE =====
window.AutoEdgeUtils = {
    showLoading: showLoading,
    showToast: showToast,
    toggleSidebar: window.AutoEdgeSidebar.toggle,
    closeSidebar: window.AutoEdgeSidebar.close,
    openSidebar: window.AutoEdgeSidebar.open
};

