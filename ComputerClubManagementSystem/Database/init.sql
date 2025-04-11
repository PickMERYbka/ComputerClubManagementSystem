-- Create the database
CREATE DATABASE computer_club_management;

-- Connect to the database
\c computer_club_management

-- Create tables

-- Table for storing information about customers
CREATE TABLE customers (
    customer_id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) UNIQUE,
    phone VARCHAR(20),
    date_of_birth DATE,
    registration_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    total_visits INTEGER DEFAULT 0,
    total_spent DECIMAL(10, 2) DEFAULT 0.00,
    membership_status VARCHAR(20) DEFAULT 'Regular' CHECK (membership_status IN ('Regular', 'VIP', 'Premium')),
    notes TEXT
);

-- Table for storing computer workstations information
CREATE TABLE computers (
    computer_id SERIAL PRIMARY KEY,
    station_name VARCHAR(50) UNIQUE NOT NULL,
    computer_type VARCHAR(20) CHECK (computer_type IN ('Gaming', 'Standard', 'Premium')),
    specifications TEXT,
    hourly_rate DECIMAL(6, 2) NOT NULL,
    status VARCHAR(20) DEFAULT 'Available' CHECK (status IN ('Available', 'Occupied', 'Maintenance', 'Reserved')),
    last_maintenance_date DATE,
    purchase_date DATE,
    is_active BOOLEAN DEFAULT TRUE
);

-- Table for storing employee information
CREATE TABLE employees (
    employee_id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) UNIQUE,
    phone VARCHAR(20),
    position VARCHAR(50) NOT NULL,
    salary DECIMAL(10, 2),
    hire_date DATE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    login_username VARCHAR(50) UNIQUE,
    login_password_hash VARCHAR(255),
    password_salt VARCHAR(50)
);

-- Table for storing session information
CREATE TABLE sessions (
    session_id SERIAL PRIMARY KEY,
    customer_id INTEGER REFERENCES customers(customer_id),
    computer_id INTEGER REFERENCES computers(computer_id),
    employee_id INTEGER REFERENCES employees(employee_id),
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP,
    duration_minutes INTEGER,
    total_cost DECIMAL(8, 2),
    payment_status VARCHAR(20) DEFAULT 'Pending' CHECK (payment_status IN ('Pending', 'Completed', 'Cancelled')),
    notes TEXT,
    is_completed BOOLEAN DEFAULT FALSE
);

-- Table for storing products/services offered in the club
CREATE TABLE products (
    product_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    category VARCHAR(50) CHECK (category IN ('Food', 'Drink', 'Gaming Accessory', 'Service', 'Other')),
    price DECIMAL(8, 2) NOT NULL,
    cost_price DECIMAL(8, 2),
    stock_quantity INTEGER DEFAULT 0,
    is_available BOOLEAN DEFAULT TRUE
);

-- Table for storing sales transactions
CREATE TABLE sales (
    sale_id SERIAL PRIMARY KEY,
    session_id INTEGER REFERENCES sessions(session_id),
    customer_id INTEGER REFERENCES customers(customer_id),
    employee_id INTEGER REFERENCES employees(employee_id),
    sale_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    total_amount DECIMAL(10, 2) NOT NULL,
    payment_method VARCHAR(20) CHECK (payment_method IN ('Cash', 'Card', 'Mobile Payment', 'Account Credit')),
    notes TEXT
);

-- Table for storing sales items
CREATE TABLE sale_items (
    sale_item_id SERIAL PRIMARY KEY,
    sale_id INTEGER REFERENCES sales(sale_id),
    product_id INTEGER REFERENCES products(product_id),
    quantity INTEGER NOT NULL,
    unit_price DECIMAL(8, 2) NOT NULL,
    subtotal DECIMAL(10, 2) NOT NULL
);

-- Table for storing reservations
CREATE TABLE reservations (
    reservation_id SERIAL PRIMARY KEY,
    customer_id INTEGER REFERENCES customers(customer_id),
    computer_id INTEGER REFERENCES computers(computer_id),
    reservation_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME,
    duration_minutes INTEGER,
    status VARCHAR(20) DEFAULT 'Confirmed' CHECK (status IN ('Confirmed', 'Cancelled', 'Completed', 'No-Show')),
    notes TEXT,
    created_by INTEGER REFERENCES employees(employee_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table for storing promotions and discounts
CREATE TABLE promotions (
    promotion_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    discount_percentage DECIMAL(5, 2),
    discount_amount DECIMAL(8, 2),
    start_date DATE NOT NULL,
    end_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    minimum_purchase DECIMAL(8, 2) DEFAULT 0.00,
    applicable_days VARCHAR(100),
    max_uses INTEGER
);

-- Table for storing events (tournaments, competitions)
CREATE TABLE events (
    event_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    event_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME,
    max_participants INTEGER,
    entry_fee DECIMAL(8, 2) DEFAULT 0.00,
    prize_pool DECIMAL(10, 2) DEFAULT 0.00,
    status VARCHAR(20) DEFAULT 'Scheduled' CHECK (status IN ('Scheduled', 'Ongoing', 'Completed', 'Cancelled')),
    created_by INTEGER REFERENCES employees(employee_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table for event participants
CREATE TABLE event_participants (
    participant_id SERIAL PRIMARY KEY,
    event_id INTEGER REFERENCES events(event_id),
    customer_id INTEGER REFERENCES customers(customer_id),
    registration_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    payment_status VARCHAR(20) DEFAULT 'Pending' CHECK (payment_status IN ('Pending', 'Completed', 'Refunded')),
    position INTEGER,
    notes TEXT
);

-- Table for storing system settings
CREATE TABLE settings (
    setting_id SERIAL PRIMARY KEY,
    setting_name VARCHAR(100) UNIQUE NOT NULL,
    setting_value TEXT,
    setting_description TEXT,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by INTEGER REFERENCES employees(employee_id)
);

-- Table for tracking expenses
CREATE TABLE expenses (
    expense_id SERIAL PRIMARY KEY,
    category VARCHAR(50) NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    description TEXT,
    expense_date DATE NOT NULL,
    recorded_by INTEGER REFERENCES employees(employee_id),
    recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Table for logging activities
CREATE TABLE activity_logs (
    log_id SERIAL PRIMARY KEY,
    employee_id INTEGER REFERENCES employees(employee_id),
    action_type VARCHAR(50) NOT NULL,
    action_details TEXT,
    ip_address VARCHAR(45),
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Add additional indexes for performance optimization
CREATE INDEX idx_sessions_customer ON sessions(customer_id);
CREATE INDEX idx_sessions_computer ON sessions(computer_id);
CREATE INDEX idx_sales_customer ON sales(customer_id);
CREATE INDEX idx_reservations_date ON reservations(reservation_date);
CREATE INDEX idx_events_date ON events(event_date);

-- Function to calculate session duration and cost
CREATE OR REPLACE FUNCTION calculate_session_cost()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.end_time IS NOT NULL AND NEW.start_time IS NOT NULL THEN
        -- Calculate duration in minutes
        NEW.duration_minutes := EXTRACT(EPOCH FROM (NEW.end_time - NEW.start_time)) / 60;
        
        -- Calculate total cost based on computer hourly rate
        SELECT (NEW.duration_minutes / 60.0) * hourly_rate 
        INTO NEW.total_cost
        FROM computers 
        WHERE computer_id = NEW.computer_id;
        
        -- Mark session as completed
        NEW.is_completed := TRUE;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger to automatically calculate cost when a session ends
CREATE TRIGGER calculate_session_cost_trigger
BEFORE UPDATE ON sessions
FOR EACH ROW
WHEN (OLD.end_time IS NULL AND NEW.end_time IS NOT NULL)
EXECUTE FUNCTION calculate_session_cost();

-- Function to update computer status based on sessions
CREATE OR REPLACE FUNCTION update_computer_status()
RETURNS TRIGGER AS $$
BEGIN
    -- When session starts, set computer status to Occupied
    IF TG_OP = 'INSERT' OR (TG_OP = 'UPDATE' AND OLD.computer_id != NEW.computer_id) THEN
        UPDATE computers SET status = 'Occupied' WHERE computer_id = NEW.computer_id;
    END IF;
    
    -- When session ends, set computer status back to Available
    IF TG_OP = 'UPDATE' AND OLD.end_time IS NULL AND NEW.end_time IS NOT NULL THEN
        UPDATE computers SET status = 'Available' WHERE computer_id = NEW.computer_id;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger to update computer status
CREATE TRIGGER update_computer_status_trigger
AFTER INSERT OR UPDATE ON sessions
FOR EACH ROW
EXECUTE FUNCTION update_computer_status();

-- Function to update customer statistics
CREATE OR REPLACE FUNCTION update_customer_stats()
RETURNS TRIGGER AS $$
BEGIN
    -- Update customer visit count and total spent
    IF NEW.is_completed = TRUE AND OLD.is_completed = FALSE THEN
        UPDATE customers 
        SET total_visits = total_visits + 1,
            total_spent = total_spent + NEW.total_cost
        WHERE customer_id = NEW.customer_id;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger to update customer statistics
CREATE TRIGGER update_customer_stats_trigger
AFTER UPDATE ON sessions
FOR EACH ROW
WHEN (NEW.is_completed = TRUE AND OLD.is_completed = FALSE)
EXECUTE FUNCTION update_customer_stats();

-- Function to create a new reservation
CREATE OR REPLACE FUNCTION create_reservation(
    p_customer_id INTEGER,
    p_computer_id INTEGER,
    p_reservation_date DATE,
    p_start_time TIME,
    p_end_time TIME,
    p_notes TEXT,
    p_employee_id INTEGER
) RETURNS INTEGER AS $$
DECLARE
    v_reservation_id INTEGER;
    v_duration_minutes INTEGER;
BEGIN
    -- Calculate duration
    v_duration_minutes := EXTRACT(EPOCH FROM (p_end_time::time - p_start_time::time)) / 60;
    
    -- Check if computer is available
    IF EXISTS (
        SELECT 1 FROM reservations 
        WHERE computer_id = p_computer_id 
        AND reservation_date = p_reservation_date 
        AND status IN ('Confirmed', 'Completed')
        AND (
            (start_time <= p_start_time AND end_time > p_start_time) OR
            (start_time < p_end_time AND end_time >= p_end_time) OR
            (start_time >= p_start_time AND end_time <= p_end_time)
        )
    ) THEN
        RAISE EXCEPTION 'Computer is already reserved for the selected time slot';
    END IF;
    
    -- Create reservation
    INSERT INTO reservations (
        customer_id, computer_id, reservation_date, start_time, end_time,
        duration_minutes, notes, created_by
    ) VALUES (
        p_customer_id, p_computer_id, p_reservation_date, p_start_time, p_end_time,
        v_duration_minutes, p_notes, p_employee_id
    ) RETURNING reservation_id INTO v_reservation_id;
    
    -- Mark computer as Reserved
    UPDATE computers SET status = 'Reserved' WHERE computer_id = p_computer_id;
    
    RETURN v_reservation_id;
END;
$$ LANGUAGE plpgsql;

-- Function to start a new session
CREATE OR REPLACE FUNCTION start_session(
    p_customer_id INTEGER,
    p_computer_id INTEGER,
    p_employee_id INTEGER,
    p_notes TEXT
) RETURNS INTEGER AS $$
DECLARE
    v_session_id INTEGER;
BEGIN
    -- Check if computer is available
    IF (SELECT status FROM computers WHERE computer_id = p_computer_id) != 'Available' THEN
        RAISE EXCEPTION 'Computer is not available';
    END IF;
    
    -- Create new session
    INSERT INTO sessions (
        customer_id, computer_id, employee_id, start_time, notes
    ) VALUES (
        p_customer_id, p_computer_id, p_employee_id, CURRENT_TIMESTAMP, p_notes
    ) RETURNING session_id INTO v_session_id;
    
    RETURN v_session_id;
END;
$$ LANGUAGE plpgsql;

-- Function to end a session
CREATE OR REPLACE FUNCTION end_session(
    p_session_id INTEGER
) RETURNS DECIMAL AS $$
DECLARE
    v_total_cost DECIMAL(8, 2);
BEGIN
    -- Update session with end time
    UPDATE sessions 
    SET end_time = CURRENT_TIMESTAMP
    WHERE session_id = p_session_id
    RETURNING total_cost INTO v_total_cost;
    
    RETURN v_total_cost;
END;
$$ LANGUAGE plpgsql;

-- Active sessions view
CREATE VIEW active_sessions_view AS
SELECT s.session_id, c.first_name || ' ' || c.last_name AS customer_name,
       comp.station_name, s.start_time,
       EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - s.start_time)) / 60 AS current_duration_minutes,
       (EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - s.start_time)) / 3600) * comp.hourly_rate AS current_cost
FROM sessions s
JOIN customers c ON s.customer_id = c.customer_id
JOIN computers comp ON s.computer_id = comp.computer_id
WHERE s.end_time IS NULL;

-- Daily revenue view
CREATE VIEW daily_revenue_view AS
WITH daily_sales AS (
    SELECT DATE(sale_date) AS sale_date,
           SUM(total_amount) AS sales_revenue
    FROM sales
    GROUP BY DATE(sale_date)
)
SELECT DATE(s.start_time) AS date,
       COUNT(s.session_id) AS total_sessions,
       SUM(s.total_cost) AS session_revenue,
       COALESCE(ds.sales_revenue, 0) AS sales_revenue,
       SUM(s.total_cost) + COALESCE(ds.sales_revenue, 0) AS total_revenue
FROM sessions s
LEFT JOIN daily_sales ds ON DATE(s.start_time) = ds.sale_date
WHERE s.is_completed = TRUE
GROUP BY DATE(s.start_time), ds.sales_revenue
ORDER BY date DESC;

-- Popular computers view
CREATE VIEW popular_computers_view AS
SELECT c.computer_id, c.station_name, c.computer_type,
       COUNT(s.session_id) AS total_sessions,
       SUM(s.duration_minutes) AS total_minutes_used,
       SUM(s.total_cost) AS total_revenue
FROM computers c
LEFT JOIN sessions s ON c.computer_id = s.computer_id
WHERE s.is_completed = TRUE
GROUP BY c.computer_id, c.station_name, c.computer_type
ORDER BY total_sessions DESC;

-- Top customers view
CREATE VIEW top_customers_view AS
SELECT c.customer_id, c.first_name || ' ' || c.last_name AS customer_name,
       c.total_visits, c.total_spent,
       c.registration_date, c.membership_status
FROM customers c
ORDER BY c.total_spent DESC
LIMIT 20;

-- Upcoming reservations view
CREATE VIEW upcoming_reservations_view AS
SELECT r.reservation_id, c.first_name || ' ' || c.last_name AS customer_name,
       comp.station_name, r.reservation_date, r.start_time, r.end_time,
       r.duration_minutes, r.status
FROM reservations r
JOIN customers c ON r.customer_id = c.customer_id
JOIN computers comp ON r.computer_id = comp.computer_id
WHERE r.reservation_date >= CURRENT_DATE AND r.status = 'Confirmed'
ORDER BY r.reservation_date, r.start_time;

-- Добавление тестовых данных
INSERT INTO employees (first_name, last_name, email, phone, position, salary, hire_date, login_username, login_password_hash) VALUES
('Админ', 'Админов', 'admin@example.com', '+7 (999) 111-11-11', 'Администратор', 50000.00, '2023-01-01', 'admin', 'admin123');

INSERT INTO customers (first_name, last_name, email, phone, membership_status) VALUES
('Иван', 'Иванов', 'ivan@example.com', '+7 (999) 123-45-67', 'Regular'),
('Петр', 'Петров', 'petr@example.com', '+7 (999) 765-43-21', 'Premium'),
('Анна', 'Сидорова', 'anna@example.com', '+7 (999) 555-55-55', 'VIP');

INSERT INTO computers (station_name, computer_type, specifications, hourly_rate) VALUES
('PC-1', 'Gaming', 'Intel i7, RTX 3080, 32GB RAM', 300.00),
('PC-2', 'Standard', 'Intel i5, GTX 1660, 16GB RAM', 200.00),
('PC-3', 'Premium', 'Intel i9, RTX 3090, 64GB RAM', 400.00);

INSERT INTO products (name, description, category, price, stock_quantity) VALUES
('Кола', 'Напиток', 'Drink', 100.00, 50),
('Чипсы', 'Закуска', 'Food', 150.00, 30),
('Энергетик', 'Напиток', 'Drink', 200.00, 20),
('Мышь игровая', 'Gaming Mouse', 'Gaming Accessory', 2500.00, 10),
('Клавиатура', 'Gaming Keyboard', 'Gaming Accessory', 3500.00, 8); 