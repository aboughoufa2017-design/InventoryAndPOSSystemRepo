--
-- PostgreSQL database dump
--


-- Dumped from database version 18.6
-- Dumped by pg_dump version 18.6

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public.warehouses DROP CONSTRAINT IF EXISTS fk_warehouses_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.warehouse_locations DROP CONSTRAINT IF EXISTS fk_warehouse_locations_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.warehouse_locations DROP CONSTRAINT IF EXISTS fk_warehouse_locations_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS fk_users_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.user_roles DROP CONSTRAINT IF EXISTS fk_user_roles_users_user_id;
ALTER TABLE IF EXISTS ONLY public.user_roles DROP CONSTRAINT IF EXISTS fk_user_roles_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.user_roles DROP CONSTRAINT IF EXISTS fk_user_roles_roles_role_id;
ALTER TABLE IF EXISTS ONLY public.units_of_measure DROP CONSTRAINT IF EXISTS fk_units_of_measure_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.transfers DROP CONSTRAINT IF EXISTS fk_transfers_warehouses_source_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.transfers DROP CONSTRAINT IF EXISTS fk_transfers_warehouses_destination_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.transfers DROP CONSTRAINT IF EXISTS fk_transfers_warehouse_locations_source_location_id;
ALTER TABLE IF EXISTS ONLY public.transfers DROP CONSTRAINT IF EXISTS fk_transfers_warehouse_locations_destination_location_id;
ALTER TABLE IF EXISTS ONLY public.transfers DROP CONSTRAINT IF EXISTS fk_transfers_users_created_by_user_id;
ALTER TABLE IF EXISTS ONLY public.transfers DROP CONSTRAINT IF EXISTS fk_transfers_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.transfer_operations DROP CONSTRAINT IF EXISTS fk_transfer_operations_transfers_transfer_id;
ALTER TABLE IF EXISTS ONLY public.transfer_operations DROP CONSTRAINT IF EXISTS fk_transfer_operations_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.transfer_lines DROP CONSTRAINT IF EXISTS fk_transfer_lines_transfers_transfer_id;
ALTER TABLE IF EXISTS ONLY public.transfer_lines DROP CONSTRAINT IF EXISTS fk_transfer_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.transfer_lines DROP CONSTRAINT IF EXISTS fk_transfer_lines_products_product_id;
ALTER TABLE IF EXISTS ONLY public.tenant_subscriptions DROP CONSTRAINT IF EXISTS fk_tenant_subscriptions_plans_plan_id;
ALTER TABLE IF EXISTS ONLY public.tenant_change_counters DROP CONSTRAINT IF EXISTS fk_tenant_change_counters_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.suppliers DROP CONSTRAINT IF EXISTS fk_suppliers_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.stock_movements DROP CONSTRAINT IF EXISTS fk_stock_movements_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.stock_movements DROP CONSTRAINT IF EXISTS fk_stock_movements_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.stock_movements DROP CONSTRAINT IF EXISTS fk_stock_movements_users_created_by_user_id;
ALTER TABLE IF EXISTS ONLY public.stock_movements DROP CONSTRAINT IF EXISTS fk_stock_movements_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.stock_movements DROP CONSTRAINT IF EXISTS fk_stock_movements_products_product_id;
ALTER TABLE IF EXISTS ONLY public.stock_counts DROP CONSTRAINT IF EXISTS fk_stock_counts_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.stock_counts DROP CONSTRAINT IF EXISTS fk_stock_counts_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.stock_counts DROP CONSTRAINT IF EXISTS fk_stock_counts_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.stock_count_lines DROP CONSTRAINT IF EXISTS fk_stock_count_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.stock_count_lines DROP CONSTRAINT IF EXISTS fk_stock_count_lines_stock_counts_stock_count_id;
ALTER TABLE IF EXISTS ONLY public.stock_count_lines DROP CONSTRAINT IF EXISTS fk_stock_count_lines_products_product_id;
ALTER TABLE IF EXISTS ONLY public.stock_balances DROP CONSTRAINT IF EXISTS fk_stock_balances_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.stock_balances DROP CONSTRAINT IF EXISTS fk_stock_balances_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.stock_balances DROP CONSTRAINT IF EXISTS fk_stock_balances_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.stock_balances DROP CONSTRAINT IF EXISTS fk_stock_balances_products_product_id;
ALTER TABLE IF EXISTS ONLY public.stock_adjustments DROP CONSTRAINT IF EXISTS fk_stock_adjustments_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.stock_adjustments DROP CONSTRAINT IF EXISTS fk_stock_adjustments_users_created_by_user_id;
ALTER TABLE IF EXISTS ONLY public.stock_adjustments DROP CONSTRAINT IF EXISTS fk_stock_adjustments_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.stock_adjustment_lines DROP CONSTRAINT IF EXISTS fk_stock_adjustment_lines_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.stock_adjustment_lines DROP CONSTRAINT IF EXISTS fk_stock_adjustment_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.stock_adjustment_lines DROP CONSTRAINT IF EXISTS fk_stock_adjustment_lines_stock_adjustments_stock_adjustment_id;
ALTER TABLE IF EXISTS ONLY public.stock_adjustment_lines DROP CONSTRAINT IF EXISTS fk_stock_adjustment_lines_products_product_id;
ALTER TABLE IF EXISTS ONLY public.sales_shipments DROP CONSTRAINT IF EXISTS fk_sales_shipments_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.sales_shipments DROP CONSTRAINT IF EXISTS fk_sales_shipments_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.sales_shipments DROP CONSTRAINT IF EXISTS fk_sales_shipments_users_created_by_user_id;
ALTER TABLE IF EXISTS ONLY public.sales_shipments DROP CONSTRAINT IF EXISTS fk_sales_shipments_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.sales_shipments DROP CONSTRAINT IF EXISTS fk_sales_shipments_sales_orders_sales_order_id;
ALTER TABLE IF EXISTS ONLY public.sales_shipment_lines DROP CONSTRAINT IF EXISTS fk_sales_shipment_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.sales_shipment_lines DROP CONSTRAINT IF EXISTS fk_sales_shipment_lines_sales_shipments_sales_shipment_id;
ALTER TABLE IF EXISTS ONLY public.sales_shipment_lines DROP CONSTRAINT IF EXISTS fk_sales_shipment_lines_sales_order_lines_sales_order_line_id;
ALTER TABLE IF EXISTS ONLY public.sales_shipment_lines DROP CONSTRAINT IF EXISTS fk_sales_shipment_lines_products_product_id;
ALTER TABLE IF EXISTS ONLY public.sales_returns DROP CONSTRAINT IF EXISTS fk_sales_returns_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.sales_returns DROP CONSTRAINT IF EXISTS fk_sales_returns_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.sales_returns DROP CONSTRAINT IF EXISTS fk_sales_returns_users_created_by_user_id;
ALTER TABLE IF EXISTS ONLY public.sales_returns DROP CONSTRAINT IF EXISTS fk_sales_returns_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.sales_returns DROP CONSTRAINT IF EXISTS fk_sales_returns_sales_orders_sales_order_id;
ALTER TABLE IF EXISTS ONLY public.sales_return_lines DROP CONSTRAINT IF EXISTS fk_sales_return_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.sales_return_lines DROP CONSTRAINT IF EXISTS fk_sales_return_lines_sales_returns_sales_return_id;
ALTER TABLE IF EXISTS ONLY public.sales_return_lines DROP CONSTRAINT IF EXISTS fk_sales_return_lines_sales_order_lines_sales_order_line_id;
ALTER TABLE IF EXISTS ONLY public.sales_return_lines DROP CONSTRAINT IF EXISTS fk_sales_return_lines_products_product_id;
ALTER TABLE IF EXISTS ONLY public.sales_orders DROP CONSTRAINT IF EXISTS fk_sales_orders_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.sales_orders DROP CONSTRAINT IF EXISTS fk_sales_orders_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.sales_orders DROP CONSTRAINT IF EXISTS fk_sales_orders_users_created_by_user_id;
ALTER TABLE IF EXISTS ONLY public.sales_orders DROP CONSTRAINT IF EXISTS fk_sales_orders_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.sales_orders DROP CONSTRAINT IF EXISTS fk_sales_orders_customers_customer_id;
ALTER TABLE IF EXISTS ONLY public.sales_order_lines DROP CONSTRAINT IF EXISTS fk_sales_order_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.sales_order_lines DROP CONSTRAINT IF EXISTS fk_sales_order_lines_sales_orders_sales_order_id;
ALTER TABLE IF EXISTS ONLY public.sales_order_lines DROP CONSTRAINT IF EXISTS fk_sales_order_lines_products_product_id;
ALTER TABLE IF EXISTS ONLY public.sales_operations DROP CONSTRAINT IF EXISTS fk_sales_operations_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.sales_operations DROP CONSTRAINT IF EXISTS fk_sales_operations_sales_orders_sales_order_id;
ALTER TABLE IF EXISTS ONLY public.roles DROP CONSTRAINT IF EXISTS fk_roles_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.role_permissions DROP CONSTRAINT IF EXISTS fk_role_permissions_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.role_permissions DROP CONSTRAINT IF EXISTS fk_role_permissions_roles_role_id;
ALTER TABLE IF EXISTS ONLY public.role_permissions DROP CONSTRAINT IF EXISTS fk_role_permissions_permissions_permission_id;
ALTER TABLE IF EXISTS ONLY public.restaurant_tables DROP CONSTRAINT IF EXISTS fk_restaurant_tables_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.restaurant_open_orders DROP CONSTRAINT IF EXISTS fk_restaurant_open_orders_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.restaurant_open_orders DROP CONSTRAINT IF EXISTS fk_restaurant_open_orders_restaurant_tables_table_id;
ALTER TABLE IF EXISTS ONLY public.refresh_tokens DROP CONSTRAINT IF EXISTS fk_refresh_tokens_users_user_id;
ALTER TABLE IF EXISTS ONLY public.refresh_tokens DROP CONSTRAINT IF EXISTS fk_refresh_tokens_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.refresh_tokens DROP CONSTRAINT IF EXISTS fk_refresh_tokens_refresh_tokens_replaced_by_token_id;
ALTER TABLE IF EXISTS ONLY public.refresh_tokens DROP CONSTRAINT IF EXISTS fk_refresh_tokens_devices_device_id;
ALTER TABLE IF EXISTS ONLY public.purchase_returns DROP CONSTRAINT IF EXISTS fk_purchase_returns_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.purchase_returns DROP CONSTRAINT IF EXISTS fk_purchase_returns_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.purchase_returns DROP CONSTRAINT IF EXISTS fk_purchase_returns_users_created_by_user_id;
ALTER TABLE IF EXISTS ONLY public.purchase_returns DROP CONSTRAINT IF EXISTS fk_purchase_returns_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.purchase_returns DROP CONSTRAINT IF EXISTS fk_purchase_returns_purchase_orders_purchase_order_id;
ALTER TABLE IF EXISTS ONLY public.purchase_return_lines DROP CONSTRAINT IF EXISTS fk_purchase_return_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.purchase_return_lines DROP CONSTRAINT IF EXISTS fk_purchase_return_lines_purchase_returns_purchase_return_id;
ALTER TABLE IF EXISTS ONLY public.purchase_return_lines DROP CONSTRAINT IF EXISTS fk_purchase_return_lines_purchase_order_lines_purchase_order_li;
ALTER TABLE IF EXISTS ONLY public.purchase_return_lines DROP CONSTRAINT IF EXISTS fk_purchase_return_lines_products_product_id;
ALTER TABLE IF EXISTS ONLY public.purchase_receipts DROP CONSTRAINT IF EXISTS fk_purchase_receipts_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.purchase_receipts DROP CONSTRAINT IF EXISTS fk_purchase_receipts_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.purchase_receipts DROP CONSTRAINT IF EXISTS fk_purchase_receipts_users_created_by_user_id;
ALTER TABLE IF EXISTS ONLY public.purchase_receipts DROP CONSTRAINT IF EXISTS fk_purchase_receipts_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.purchase_receipts DROP CONSTRAINT IF EXISTS fk_purchase_receipts_purchase_orders_purchase_order_id;
ALTER TABLE IF EXISTS ONLY public.purchase_receipt_lines DROP CONSTRAINT IF EXISTS fk_purchase_receipt_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.purchase_receipt_lines DROP CONSTRAINT IF EXISTS fk_purchase_receipt_lines_purchase_receipts_purchase_receipt_id;
ALTER TABLE IF EXISTS ONLY public.purchase_receipt_lines DROP CONSTRAINT IF EXISTS fk_purchase_receipt_lines_purchase_order_lines_purchase_order_l;
ALTER TABLE IF EXISTS ONLY public.purchase_receipt_lines DROP CONSTRAINT IF EXISTS fk_purchase_receipt_lines_products_product_id;
ALTER TABLE IF EXISTS ONLY public.purchase_orders DROP CONSTRAINT IF EXISTS fk_purchase_orders_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.purchase_orders DROP CONSTRAINT IF EXISTS fk_purchase_orders_warehouse_locations_location_id;
ALTER TABLE IF EXISTS ONLY public.purchase_orders DROP CONSTRAINT IF EXISTS fk_purchase_orders_users_created_by_user_id;
ALTER TABLE IF EXISTS ONLY public.purchase_orders DROP CONSTRAINT IF EXISTS fk_purchase_orders_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.purchase_orders DROP CONSTRAINT IF EXISTS fk_purchase_orders_suppliers_supplier_id;
ALTER TABLE IF EXISTS ONLY public.purchase_order_lines DROP CONSTRAINT IF EXISTS fk_purchase_order_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.purchase_order_lines DROP CONSTRAINT IF EXISTS fk_purchase_order_lines_purchase_orders_purchase_order_id;
ALTER TABLE IF EXISTS ONLY public.purchase_order_lines DROP CONSTRAINT IF EXISTS fk_purchase_order_lines_products_product_id;
ALTER TABLE IF EXISTS ONLY public.purchase_operations DROP CONSTRAINT IF EXISTS fk_purchase_operations_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.purchase_operations DROP CONSTRAINT IF EXISTS fk_purchase_operations_purchase_orders_purchase_order_id;
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS fk_products_units_of_measure_unit_of_measure_id;
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS fk_products_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS fk_products_product_categories_category_id;
ALTER TABLE IF EXISTS ONLY public.product_prices DROP CONSTRAINT IF EXISTS fk_product_prices_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.product_prices DROP CONSTRAINT IF EXISTS fk_product_prices_price_lists_price_list_id;
ALTER TABLE IF EXISTS ONLY public.product_categories DROP CONSTRAINT IF EXISTS fk_product_categories_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.product_categories DROP CONSTRAINT IF EXISTS fk_product_categories_product_categories_parent_category_id;
ALTER TABLE IF EXISTS ONLY public.product_barcodes DROP CONSTRAINT IF EXISTS fk_product_barcodes_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.product_barcodes DROP CONSTRAINT IF EXISTS fk_product_barcodes_products_product_id;
ALTER TABLE IF EXISTS ONLY public.processed_sync_operations DROP CONSTRAINT IF EXISTS fk_processed_sync_operations_users_user_id;
ALTER TABLE IF EXISTS ONLY public.processed_sync_operations DROP CONSTRAINT IF EXISTS fk_processed_sync_operations_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.processed_sync_operations DROP CONSTRAINT IF EXISTS fk_processed_sync_operations_devices_device_id;
ALTER TABLE IF EXISTS ONLY public.price_lists DROP CONSTRAINT IF EXISTS fk_price_lists_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.pos_sales DROP CONSTRAINT IF EXISTS fk_pos_sales_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.pos_sale_lines DROP CONSTRAINT IF EXISTS fk_pos_sale_lines_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.pos_sale_lines DROP CONSTRAINT IF EXISTS fk_pos_sale_lines_pos_sales_pos_sale_id;
ALTER TABLE IF EXISTS ONLY public.pos_payments DROP CONSTRAINT IF EXISTS fk_pos_payments_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.pos_payments DROP CONSTRAINT IF EXISTS fk_pos_payments_pos_sales_pos_sale_id;
ALTER TABLE IF EXISTS ONLY public.pos_inventory_conflicts DROP CONSTRAINT IF EXISTS fk_pos_inventory_conflicts_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.devices DROP CONSTRAINT IF EXISTS fk_devices_warehouses_warehouse_id;
ALTER TABLE IF EXISTS ONLY public.devices DROP CONSTRAINT IF EXISTS fk_devices_users_user_id;
ALTER TABLE IF EXISTS ONLY public.devices DROP CONSTRAINT IF EXISTS fk_devices_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.customers DROP CONSTRAINT IF EXISTS fk_customers_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.change_log DROP CONSTRAINT IF EXISTS fk_change_log_users_user_id;
ALTER TABLE IF EXISTS ONLY public.change_log DROP CONSTRAINT IF EXISTS fk_change_log_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.change_log DROP CONSTRAINT IF EXISTS fk_change_log_devices_device_id;
ALTER TABLE IF EXISTS ONLY public.cash_shifts DROP CONSTRAINT IF EXISTS fk_cash_shifts_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.cash_movements DROP CONSTRAINT IF EXISTS fk_cash_movements_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.cash_movements DROP CONSTRAINT IF EXISTS fk_cash_movements_cash_shifts_cash_shift_id;
ALTER TABLE IF EXISTS ONLY public.audit_logs DROP CONSTRAINT IF EXISTS fk_audit_logs_users_user_id;
ALTER TABLE IF EXISTS ONLY public.audit_logs DROP CONSTRAINT IF EXISTS fk_audit_logs_tenants_tenant_id;
ALTER TABLE IF EXISTS ONLY public.audit_logs DROP CONSTRAINT IF EXISTS fk_audit_logs_devices_device_id;
DROP INDEX IF EXISTS public.ix_warehouses_tenant_id_name;
DROP INDEX IF EXISTS public.ix_warehouses_tenant_id_is_active;
DROP INDEX IF EXISTS public.ix_warehouses_tenant_id_code;
DROP INDEX IF EXISTS public.ix_warehouses_tenant_id;
DROP INDEX IF EXISTS public.ix_warehouse_locations_warehouse_id;
DROP INDEX IF EXISTS public.ix_warehouse_locations_tenant_id_warehouse_id_kind;
DROP INDEX IF EXISTS public.ix_warehouse_locations_tenant_id_warehouse_id_code;
DROP INDEX IF EXISTS public.ix_warehouse_locations_tenant_id_warehouse_id;
DROP INDEX IF EXISTS public.ix_warehouse_locations_tenant_id;
DROP INDEX IF EXISTS public.ix_users_tenant_id_is_active;
DROP INDEX IF EXISTS public.ix_users_tenant_id_email;
DROP INDEX IF EXISTS public.ix_users_tenant_id;
DROP INDEX IF EXISTS public.ix_user_roles_user_id_role_id;
DROP INDEX IF EXISTS public.ix_user_roles_tenant_id_user_id;
DROP INDEX IF EXISTS public.ix_user_roles_tenant_id;
DROP INDEX IF EXISTS public.ix_user_roles_role_id;
DROP INDEX IF EXISTS public.ix_units_of_measure_tenant_id_is_active;
DROP INDEX IF EXISTS public.ix_units_of_measure_tenant_id_code;
DROP INDEX IF EXISTS public.ix_units_of_measure_tenant_id;
DROP INDEX IF EXISTS public.ix_transfers_tenant_id_status;
DROP INDEX IF EXISTS public.ix_transfers_tenant_id_source_warehouse_id;
DROP INDEX IF EXISTS public.ix_transfers_tenant_id_destination_warehouse_id;
DROP INDEX IF EXISTS public.ix_transfers_tenant_id;
DROP INDEX IF EXISTS public.ix_transfers_source_warehouse_id;
DROP INDEX IF EXISTS public.ix_transfers_source_location_id;
DROP INDEX IF EXISTS public.ix_transfers_dispatch_operation_id;
DROP INDEX IF EXISTS public.ix_transfers_destination_warehouse_id;
DROP INDEX IF EXISTS public.ix_transfers_destination_location_id;
DROP INDEX IF EXISTS public.ix_transfers_created_by_user_id;
DROP INDEX IF EXISTS public.ix_transfer_operations_transfer_id;
DROP INDEX IF EXISTS public.ix_transfer_operations_tenant_id_transfer_id;
DROP INDEX IF EXISTS public.ix_transfer_operations_tenant_id;
DROP INDEX IF EXISTS public.ix_transfer_lines_transfer_id_product_id;
DROP INDEX IF EXISTS public.ix_transfer_lines_tenant_id;
DROP INDEX IF EXISTS public.ix_transfer_lines_product_id;
DROP INDEX IF EXISTS public.ix_tenants_name;
DROP INDEX IF EXISTS public.ix_tenants_code;
DROP INDEX IF EXISTS public.ix_tenant_subscriptions_tenant_id_status;
DROP INDEX IF EXISTS public.ix_tenant_subscriptions_tenant_id;
DROP INDEX IF EXISTS public.ix_tenant_subscriptions_plan_id;
DROP INDEX IF EXISTS public.ix_suppliers_tenant_id_is_active;
DROP INDEX IF EXISTS public.ix_suppliers_tenant_id_code;
DROP INDEX IF EXISTS public.ix_suppliers_tenant_id;
DROP INDEX IF EXISTS public.ix_stock_movements_warehouse_id;
DROP INDEX IF EXISTS public.ix_stock_movements_transfer_line_id;
DROP INDEX IF EXISTS public.ix_stock_movements_tenant_id_warehouse_id_location_id;
DROP INDEX IF EXISTS public.ix_stock_movements_tenant_id_source_document_type_source_docume;
DROP INDEX IF EXISTS public.ix_stock_movements_tenant_id_product_id_occurred_at_utc;
DROP INDEX IF EXISTS public.ix_stock_movements_tenant_id;
DROP INDEX IF EXISTS public.ix_stock_movements_product_id;
DROP INDEX IF EXISTS public.ix_stock_movements_location_id;
DROP INDEX IF EXISTS public.ix_stock_movements_created_by_user_id;
DROP INDEX IF EXISTS public.ix_stock_counts_warehouse_id;
DROP INDEX IF EXISTS public.ix_stock_counts_tenant_id_warehouse_id_status;
DROP INDEX IF EXISTS public.ix_stock_counts_tenant_id;
DROP INDEX IF EXISTS public.ix_stock_counts_location_id;
DROP INDEX IF EXISTS public.ix_stock_count_lines_tenant_id;
DROP INDEX IF EXISTS public.ix_stock_count_lines_stock_count_id_product_id;
DROP INDEX IF EXISTS public.ix_stock_count_lines_product_id;
DROP INDEX IF EXISTS public.ix_stock_balances_warehouse_id;
DROP INDEX IF EXISTS public.ix_stock_balances_tenant_id_warehouse_id_product_id;
DROP INDEX IF EXISTS public.ix_stock_balances_tenant_id_product_id_warehouse_id_location_id;
DROP INDEX IF EXISTS public.ix_stock_balances_tenant_id;
DROP INDEX IF EXISTS public.ix_stock_balances_product_id;
DROP INDEX IF EXISTS public.ix_stock_balances_location_id;
DROP INDEX IF EXISTS public.ix_stock_adjustments_warehouse_id;
DROP INDEX IF EXISTS public.ix_stock_adjustments_tenant_id_warehouse_id_status;
DROP INDEX IF EXISTS public.ix_stock_adjustments_tenant_id;
DROP INDEX IF EXISTS public.ix_stock_adjustments_created_by_user_id;
DROP INDEX IF EXISTS public.ix_stock_adjustment_lines_tenant_id;
DROP INDEX IF EXISTS public.ix_stock_adjustment_lines_stock_adjustment_id_product_id_locati;
DROP INDEX IF EXISTS public.ix_stock_adjustment_lines_product_id;
DROP INDEX IF EXISTS public.ix_stock_adjustment_lines_location_id;
DROP INDEX IF EXISTS public.ix_sales_shipments_warehouse_id;
DROP INDEX IF EXISTS public.ix_sales_shipments_tenant_id_sales_order_id;
DROP INDEX IF EXISTS public.ix_sales_shipments_tenant_id;
DROP INDEX IF EXISTS public.ix_sales_shipments_sales_order_id;
DROP INDEX IF EXISTS public.ix_sales_shipments_operation_id;
DROP INDEX IF EXISTS public.ix_sales_shipments_location_id;
DROP INDEX IF EXISTS public.ix_sales_shipments_created_by_user_id;
DROP INDEX IF EXISTS public.ix_sales_shipment_lines_tenant_id;
DROP INDEX IF EXISTS public.ix_sales_shipment_lines_sales_shipment_id_sales_order_line_id;
DROP INDEX IF EXISTS public.ix_sales_shipment_lines_sales_order_line_id;
DROP INDEX IF EXISTS public.ix_sales_shipment_lines_product_id;
DROP INDEX IF EXISTS public.ix_sales_returns_warehouse_id;
DROP INDEX IF EXISTS public.ix_sales_returns_tenant_id_sales_order_id;
DROP INDEX IF EXISTS public.ix_sales_returns_tenant_id;
DROP INDEX IF EXISTS public.ix_sales_returns_sales_order_id;
DROP INDEX IF EXISTS public.ix_sales_returns_operation_id;
DROP INDEX IF EXISTS public.ix_sales_returns_location_id;
DROP INDEX IF EXISTS public.ix_sales_returns_created_by_user_id;
DROP INDEX IF EXISTS public.ix_sales_return_lines_tenant_id;
DROP INDEX IF EXISTS public.ix_sales_return_lines_sales_return_id_sales_order_line_id;
DROP INDEX IF EXISTS public.ix_sales_return_lines_sales_order_line_id;
DROP INDEX IF EXISTS public.ix_sales_return_lines_product_id;
DROP INDEX IF EXISTS public.ix_sales_orders_warehouse_id;
DROP INDEX IF EXISTS public.ix_sales_orders_tenant_id_status;
DROP INDEX IF EXISTS public.ix_sales_orders_tenant_id_customer_id;
DROP INDEX IF EXISTS public.ix_sales_orders_tenant_id;
DROP INDEX IF EXISTS public.ix_sales_orders_location_id;
DROP INDEX IF EXISTS public.ix_sales_orders_customer_id;
DROP INDEX IF EXISTS public.ix_sales_orders_created_by_user_id;
DROP INDEX IF EXISTS public.ix_sales_order_lines_tenant_id;
DROP INDEX IF EXISTS public.ix_sales_order_lines_sales_order_id_product_id;
DROP INDEX IF EXISTS public.ix_sales_order_lines_product_id;
DROP INDEX IF EXISTS public.ix_sales_operations_tenant_id_sales_order_id;
DROP INDEX IF EXISTS public.ix_sales_operations_tenant_id;
DROP INDEX IF EXISTS public.ix_sales_operations_sales_order_id;
DROP INDEX IF EXISTS public.ix_roles_tenant_id_name;
DROP INDEX IF EXISTS public.ix_roles_tenant_id_code;
DROP INDEX IF EXISTS public.ix_roles_tenant_id;
DROP INDEX IF EXISTS public.ix_role_permissions_tenant_id_role_id;
DROP INDEX IF EXISTS public.ix_role_permissions_tenant_id;
DROP INDEX IF EXISTS public.ix_role_permissions_role_id_permission_id;
DROP INDEX IF EXISTS public.ix_role_permissions_permission_id;
DROP INDEX IF EXISTS public.ix_restaurant_tables_tenant_id_warehouse_id_code;
DROP INDEX IF EXISTS public.ix_restaurant_open_orders_tenant_id_warehouse_id_status;
DROP INDEX IF EXISTS public.ix_restaurant_open_orders_table_id;
DROP INDEX IF EXISTS public.ix_refresh_tokens_user_id;
DROP INDEX IF EXISTS public.ix_refresh_tokens_token_hash;
DROP INDEX IF EXISTS public.ix_refresh_tokens_tenant_id_user_id;
DROP INDEX IF EXISTS public.ix_refresh_tokens_tenant_id;
DROP INDEX IF EXISTS public.ix_refresh_tokens_replaced_by_token_id;
DROP INDEX IF EXISTS public.ix_refresh_tokens_family_id;
DROP INDEX IF EXISTS public.ix_refresh_tokens_device_id;
DROP INDEX IF EXISTS public.ix_purchase_returns_warehouse_id;
DROP INDEX IF EXISTS public.ix_purchase_returns_tenant_id_purchase_order_id;
DROP INDEX IF EXISTS public.ix_purchase_returns_tenant_id;
DROP INDEX IF EXISTS public.ix_purchase_returns_purchase_order_id;
DROP INDEX IF EXISTS public.ix_purchase_returns_operation_id;
DROP INDEX IF EXISTS public.ix_purchase_returns_location_id;
DROP INDEX IF EXISTS public.ix_purchase_returns_created_by_user_id;
DROP INDEX IF EXISTS public.ix_purchase_return_lines_tenant_id;
DROP INDEX IF EXISTS public.ix_purchase_return_lines_purchase_return_id_purchase_order_line;
DROP INDEX IF EXISTS public.ix_purchase_return_lines_purchase_order_line_id;
DROP INDEX IF EXISTS public.ix_purchase_return_lines_product_id;
DROP INDEX IF EXISTS public.ix_purchase_receipts_warehouse_id;
DROP INDEX IF EXISTS public.ix_purchase_receipts_tenant_id_purchase_order_id;
DROP INDEX IF EXISTS public.ix_purchase_receipts_tenant_id;
DROP INDEX IF EXISTS public.ix_purchase_receipts_purchase_order_id;
DROP INDEX IF EXISTS public.ix_purchase_receipts_operation_id;
DROP INDEX IF EXISTS public.ix_purchase_receipts_location_id;
DROP INDEX IF EXISTS public.ix_purchase_receipts_created_by_user_id;
DROP INDEX IF EXISTS public.ix_purchase_receipt_lines_tenant_id;
DROP INDEX IF EXISTS public.ix_purchase_receipt_lines_purchase_receipt_id_purchase_order_li;
DROP INDEX IF EXISTS public.ix_purchase_receipt_lines_purchase_order_line_id;
DROP INDEX IF EXISTS public.ix_purchase_receipt_lines_product_id;
DROP INDEX IF EXISTS public.ix_purchase_orders_warehouse_id;
DROP INDEX IF EXISTS public.ix_purchase_orders_tenant_id_supplier_id;
DROP INDEX IF EXISTS public.ix_purchase_orders_tenant_id_status;
DROP INDEX IF EXISTS public.ix_purchase_orders_tenant_id;
DROP INDEX IF EXISTS public.ix_purchase_orders_supplier_id;
DROP INDEX IF EXISTS public.ix_purchase_orders_location_id;
DROP INDEX IF EXISTS public.ix_purchase_orders_created_by_user_id;
DROP INDEX IF EXISTS public.ix_purchase_order_lines_tenant_id;
DROP INDEX IF EXISTS public.ix_purchase_order_lines_purchase_order_id_product_id;
DROP INDEX IF EXISTS public.ix_purchase_order_lines_product_id;
DROP INDEX IF EXISTS public.ix_purchase_operations_tenant_id_purchase_order_id;
DROP INDEX IF EXISTS public.ix_purchase_operations_tenant_id;
DROP INDEX IF EXISTS public.ix_purchase_operations_purchase_order_id;
DROP INDEX IF EXISTS public.ix_products_unit_of_measure_id;
DROP INDEX IF EXISTS public.ix_products_tenant_id_sku;
DROP INDEX IF EXISTS public.ix_products_tenant_id_name;
DROP INDEX IF EXISTS public.ix_products_tenant_id_is_active;
DROP INDEX IF EXISTS public.ix_products_tenant_id_category_id;
DROP INDEX IF EXISTS public.ix_products_tenant_id;
DROP INDEX IF EXISTS public.ix_products_category_id;
DROP INDEX IF EXISTS public.ix_product_prices_tenant_id_price_list_id_product_id_min_quant;
DROP INDEX IF EXISTS public.ix_product_prices_price_list_id;
DROP INDEX IF EXISTS public.ix_product_categories_tenant_id_is_active;
DROP INDEX IF EXISTS public.ix_product_categories_tenant_id_code;
DROP INDEX IF EXISTS public.ix_product_categories_tenant_id;
DROP INDEX IF EXISTS public.ix_product_categories_parent_category_id;
DROP INDEX IF EXISTS public.ix_product_barcodes_tenant_id_product_id;
DROP INDEX IF EXISTS public.ix_product_barcodes_tenant_id_barcode;
DROP INDEX IF EXISTS public.ix_product_barcodes_tenant_id;
DROP INDEX IF EXISTS public.ix_product_barcodes_product_id;
DROP INDEX IF EXISTS public.ix_processed_sync_operations_user_id;
DROP INDEX IF EXISTS public.ix_processed_sync_operations_tenant_id_entity_type_entity_id;
DROP INDEX IF EXISTS public.ix_processed_sync_operations_tenant_id_device_id;
DROP INDEX IF EXISTS public.ix_processed_sync_operations_tenant_id_created_at_utc;
DROP INDEX IF EXISTS public.ix_processed_sync_operations_tenant_id;
DROP INDEX IF EXISTS public.ix_processed_sync_operations_device_id;
DROP INDEX IF EXISTS public.ix_price_lists_tenant_id_code;
DROP INDEX IF EXISTS public.ix_pos_sales_tenant_id_warehouse_id_sold_at_utc;
DROP INDEX IF EXISTS public.ix_pos_sales_tenant_id_receipt_number;
DROP INDEX IF EXISTS public.ix_pos_sales_tenant_id_client_command_id;
DROP INDEX IF EXISTS public.ix_pos_sale_lines_tenant_id_pos_sale_id;
DROP INDEX IF EXISTS public.ix_pos_sale_lines_pos_sale_id;
DROP INDEX IF EXISTS public.ix_pos_payments_tenant_id_pos_sale_id;
DROP INDEX IF EXISTS public.ix_pos_payments_pos_sale_id;
DROP INDEX IF EXISTS public.ix_pos_inventory_conflicts_tenant_id_status;
DROP INDEX IF EXISTS public.ix_platform_operators_email;
DROP INDEX IF EXISTS public.ix_plans_code;
DROP INDEX IF EXISTS public.ix_permissions_code;
DROP INDEX IF EXISTS public.ix_devices_warehouse_id;
DROP INDEX IF EXISTS public.ix_devices_user_id;
DROP INDEX IF EXISTS public.ix_devices_tenant_id_warehouse_id;
DROP INDEX IF EXISTS public.ix_devices_tenant_id_user_id;
DROP INDEX IF EXISTS public.ix_devices_tenant_id_status;
DROP INDEX IF EXISTS public.ix_devices_tenant_id_device_key;
DROP INDEX IF EXISTS public.ix_devices_tenant_id;
DROP INDEX IF EXISTS public.ix_customers_tenant_id_is_active;
DROP INDEX IF EXISTS public.ix_customers_tenant_id_code;
DROP INDEX IF EXISTS public.ix_customers_tenant_id;
DROP INDEX IF EXISTS public.ix_change_log_user_id;
DROP INDEX IF EXISTS public.ix_change_log_tenant_id_entity_type_entity_id;
DROP INDEX IF EXISTS public.ix_change_log_tenant_id_change_version;
DROP INDEX IF EXISTS public.ix_change_log_tenant_id;
DROP INDEX IF EXISTS public.ix_change_log_device_id;
DROP INDEX IF EXISTS public.ix_cash_shifts_tenant_id_warehouse_id_status;
DROP INDEX IF EXISTS public.ix_cash_movements_tenant_id_cash_shift_id;
DROP INDEX IF EXISTS public.ix_cash_movements_cash_shift_id;
DROP INDEX IF EXISTS public.ix_audit_logs_user_id;
DROP INDEX IF EXISTS public.ix_audit_logs_tenant_id_user_id;
DROP INDEX IF EXISTS public.ix_audit_logs_tenant_id_occurred_at_utc;
DROP INDEX IF EXISTS public.ix_audit_logs_tenant_id_entity_type_entity_id;
DROP INDEX IF EXISTS public.ix_audit_logs_tenant_id;
DROP INDEX IF EXISTS public.ix_audit_logs_device_id;
DROP INDEX IF EXISTS public.ix_audit_logs_correlation_id;
ALTER TABLE IF EXISTS ONLY public.warehouses DROP CONSTRAINT IF EXISTS pk_warehouses;
ALTER TABLE IF EXISTS ONLY public.warehouse_locations DROP CONSTRAINT IF EXISTS pk_warehouse_locations;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS pk_users;
ALTER TABLE IF EXISTS ONLY public.user_roles DROP CONSTRAINT IF EXISTS pk_user_roles;
ALTER TABLE IF EXISTS ONLY public.units_of_measure DROP CONSTRAINT IF EXISTS pk_units_of_measure;
ALTER TABLE IF EXISTS ONLY public.transfers DROP CONSTRAINT IF EXISTS pk_transfers;
ALTER TABLE IF EXISTS ONLY public.transfer_operations DROP CONSTRAINT IF EXISTS pk_transfer_operations;
ALTER TABLE IF EXISTS ONLY public.transfer_lines DROP CONSTRAINT IF EXISTS pk_transfer_lines;
ALTER TABLE IF EXISTS ONLY public.tenants DROP CONSTRAINT IF EXISTS pk_tenants;
ALTER TABLE IF EXISTS ONLY public.tenant_subscriptions DROP CONSTRAINT IF EXISTS pk_tenant_subscriptions;
ALTER TABLE IF EXISTS ONLY public.tenant_change_counters DROP CONSTRAINT IF EXISTS pk_tenant_change_counters;
ALTER TABLE IF EXISTS ONLY public.suppliers DROP CONSTRAINT IF EXISTS pk_suppliers;
ALTER TABLE IF EXISTS ONLY public.stock_movements DROP CONSTRAINT IF EXISTS pk_stock_movements;
ALTER TABLE IF EXISTS ONLY public.stock_counts DROP CONSTRAINT IF EXISTS pk_stock_counts;
ALTER TABLE IF EXISTS ONLY public.stock_count_lines DROP CONSTRAINT IF EXISTS pk_stock_count_lines;
ALTER TABLE IF EXISTS ONLY public.stock_balances DROP CONSTRAINT IF EXISTS pk_stock_balances;
ALTER TABLE IF EXISTS ONLY public.stock_adjustments DROP CONSTRAINT IF EXISTS pk_stock_adjustments;
ALTER TABLE IF EXISTS ONLY public.stock_adjustment_lines DROP CONSTRAINT IF EXISTS pk_stock_adjustment_lines;
ALTER TABLE IF EXISTS ONLY public.sales_shipments DROP CONSTRAINT IF EXISTS pk_sales_shipments;
ALTER TABLE IF EXISTS ONLY public.sales_shipment_lines DROP CONSTRAINT IF EXISTS pk_sales_shipment_lines;
ALTER TABLE IF EXISTS ONLY public.sales_returns DROP CONSTRAINT IF EXISTS pk_sales_returns;
ALTER TABLE IF EXISTS ONLY public.sales_return_lines DROP CONSTRAINT IF EXISTS pk_sales_return_lines;
ALTER TABLE IF EXISTS ONLY public.sales_orders DROP CONSTRAINT IF EXISTS pk_sales_orders;
ALTER TABLE IF EXISTS ONLY public.sales_order_lines DROP CONSTRAINT IF EXISTS pk_sales_order_lines;
ALTER TABLE IF EXISTS ONLY public.sales_operations DROP CONSTRAINT IF EXISTS pk_sales_operations;
ALTER TABLE IF EXISTS ONLY public.roles DROP CONSTRAINT IF EXISTS pk_roles;
ALTER TABLE IF EXISTS ONLY public.role_permissions DROP CONSTRAINT IF EXISTS pk_role_permissions;
ALTER TABLE IF EXISTS ONLY public.restaurant_tables DROP CONSTRAINT IF EXISTS pk_restaurant_tables;
ALTER TABLE IF EXISTS ONLY public.restaurant_open_orders DROP CONSTRAINT IF EXISTS pk_restaurant_open_orders;
ALTER TABLE IF EXISTS ONLY public.refresh_tokens DROP CONSTRAINT IF EXISTS pk_refresh_tokens;
ALTER TABLE IF EXISTS ONLY public.purchase_returns DROP CONSTRAINT IF EXISTS pk_purchase_returns;
ALTER TABLE IF EXISTS ONLY public.purchase_return_lines DROP CONSTRAINT IF EXISTS pk_purchase_return_lines;
ALTER TABLE IF EXISTS ONLY public.purchase_receipts DROP CONSTRAINT IF EXISTS pk_purchase_receipts;
ALTER TABLE IF EXISTS ONLY public.purchase_receipt_lines DROP CONSTRAINT IF EXISTS pk_purchase_receipt_lines;
ALTER TABLE IF EXISTS ONLY public.purchase_orders DROP CONSTRAINT IF EXISTS pk_purchase_orders;
ALTER TABLE IF EXISTS ONLY public.purchase_order_lines DROP CONSTRAINT IF EXISTS pk_purchase_order_lines;
ALTER TABLE IF EXISTS ONLY public.purchase_operations DROP CONSTRAINT IF EXISTS pk_purchase_operations;
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS pk_products;
ALTER TABLE IF EXISTS ONLY public.product_prices DROP CONSTRAINT IF EXISTS pk_product_prices;
ALTER TABLE IF EXISTS ONLY public.product_categories DROP CONSTRAINT IF EXISTS pk_product_categories;
ALTER TABLE IF EXISTS ONLY public.product_barcodes DROP CONSTRAINT IF EXISTS pk_product_barcodes;
ALTER TABLE IF EXISTS ONLY public.processed_sync_operations DROP CONSTRAINT IF EXISTS pk_processed_sync_operations;
ALTER TABLE IF EXISTS ONLY public.price_lists DROP CONSTRAINT IF EXISTS pk_price_lists;
ALTER TABLE IF EXISTS ONLY public.pos_sales DROP CONSTRAINT IF EXISTS pk_pos_sales;
ALTER TABLE IF EXISTS ONLY public.pos_sale_lines DROP CONSTRAINT IF EXISTS pk_pos_sale_lines;
ALTER TABLE IF EXISTS ONLY public.pos_payments DROP CONSTRAINT IF EXISTS pk_pos_payments;
ALTER TABLE IF EXISTS ONLY public.pos_inventory_conflicts DROP CONSTRAINT IF EXISTS pk_pos_inventory_conflicts;
ALTER TABLE IF EXISTS ONLY public.platform_operators DROP CONSTRAINT IF EXISTS pk_platform_operators;
ALTER TABLE IF EXISTS ONLY public.plans DROP CONSTRAINT IF EXISTS pk_plans;
ALTER TABLE IF EXISTS ONLY public.permissions DROP CONSTRAINT IF EXISTS pk_permissions;
ALTER TABLE IF EXISTS ONLY public.devices DROP CONSTRAINT IF EXISTS pk_devices;
ALTER TABLE IF EXISTS ONLY public.customers DROP CONSTRAINT IF EXISTS pk_customers;
ALTER TABLE IF EXISTS ONLY public.change_log DROP CONSTRAINT IF EXISTS pk_change_log;
ALTER TABLE IF EXISTS ONLY public.cash_shifts DROP CONSTRAINT IF EXISTS pk_cash_shifts;
ALTER TABLE IF EXISTS ONLY public.cash_movements DROP CONSTRAINT IF EXISTS pk_cash_movements;
ALTER TABLE IF EXISTS ONLY public.audit_logs DROP CONSTRAINT IF EXISTS pk_audit_logs;
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS pk___ef_migrations_history;
DROP TABLE IF EXISTS public.warehouses;
DROP TABLE IF EXISTS public.warehouse_locations;
DROP TABLE IF EXISTS public.users;
DROP TABLE IF EXISTS public.user_roles;
DROP TABLE IF EXISTS public.units_of_measure;
DROP TABLE IF EXISTS public.transfers;
DROP TABLE IF EXISTS public.transfer_operations;
DROP TABLE IF EXISTS public.transfer_lines;
DROP TABLE IF EXISTS public.tenants;
DROP TABLE IF EXISTS public.tenant_subscriptions;
DROP TABLE IF EXISTS public.tenant_change_counters;
DROP TABLE IF EXISTS public.suppliers;
DROP TABLE IF EXISTS public.stock_movements;
DROP TABLE IF EXISTS public.stock_counts;
DROP TABLE IF EXISTS public.stock_count_lines;
DROP TABLE IF EXISTS public.stock_balances;
DROP TABLE IF EXISTS public.stock_adjustments;
DROP TABLE IF EXISTS public.stock_adjustment_lines;
DROP TABLE IF EXISTS public.sales_shipments;
DROP TABLE IF EXISTS public.sales_shipment_lines;
DROP TABLE IF EXISTS public.sales_returns;
DROP TABLE IF EXISTS public.sales_return_lines;
DROP TABLE IF EXISTS public.sales_orders;
DROP TABLE IF EXISTS public.sales_order_lines;
DROP TABLE IF EXISTS public.sales_operations;
DROP TABLE IF EXISTS public.roles;
DROP TABLE IF EXISTS public.role_permissions;
DROP TABLE IF EXISTS public.restaurant_tables;
DROP TABLE IF EXISTS public.restaurant_open_orders;
DROP TABLE IF EXISTS public.refresh_tokens;
DROP TABLE IF EXISTS public.purchase_returns;
DROP TABLE IF EXISTS public.purchase_return_lines;
DROP TABLE IF EXISTS public.purchase_receipts;
DROP TABLE IF EXISTS public.purchase_receipt_lines;
DROP TABLE IF EXISTS public.purchase_orders;
DROP TABLE IF EXISTS public.purchase_order_lines;
DROP TABLE IF EXISTS public.purchase_operations;
DROP TABLE IF EXISTS public.products;
DROP TABLE IF EXISTS public.product_prices;
DROP TABLE IF EXISTS public.product_categories;
DROP TABLE IF EXISTS public.product_barcodes;
DROP TABLE IF EXISTS public.processed_sync_operations;
DROP TABLE IF EXISTS public.price_lists;
DROP TABLE IF EXISTS public.pos_sales;
DROP TABLE IF EXISTS public.pos_sale_lines;
DROP TABLE IF EXISTS public.pos_payments;
DROP TABLE IF EXISTS public.pos_inventory_conflicts;
DROP TABLE IF EXISTS public.platform_operators;
DROP TABLE IF EXISTS public.plans;
DROP TABLE IF EXISTS public.permissions;
DROP TABLE IF EXISTS public.devices;
DROP TABLE IF EXISTS public.customers;
DROP TABLE IF EXISTS public.change_log;
DROP TABLE IF EXISTS public.cash_shifts;
DROP TABLE IF EXISTS public.cash_movements;
DROP TABLE IF EXISTS public.audit_logs;
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
-- *not* dropping schema, since initdb creates it
--
-- Name: public; Type: SCHEMA; Schema: -; Owner: -
--

-- *not* creating schema, since initdb creates it


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL
);


--
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.audit_logs (
    id uuid NOT NULL,
    tenant_id uuid,
    user_id uuid,
    device_id uuid,
    action character varying(128) NOT NULL,
    entity_type character varying(128) NOT NULL,
    entity_id uuid,
    occurred_at_utc timestamp with time zone NOT NULL,
    correlation_id character varying(64),
    ip_address character varying(64),
    metadata_json jsonb
);


--
-- Name: cash_movements; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cash_movements (
    id uuid NOT NULL,
    cash_shift_id uuid NOT NULL,
    type character varying(32) NOT NULL,
    amount numeric(19,4) NOT NULL,
    reason character varying(512),
    created_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: cash_shifts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cash_shifts (
    id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    device_id uuid,
    cashier_user_id uuid NOT NULL,
    status character varying(32) NOT NULL,
    opening_float numeric(19,4) NOT NULL,
    closing_counted_cash numeric(19,4),
    opened_at_utc timestamp with time zone NOT NULL,
    closed_at_utc timestamp with time zone,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: change_log; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.change_log (
    id uuid NOT NULL,
    change_version bigint NOT NULL,
    entity_type character varying(128) NOT NULL,
    entity_id uuid NOT NULL,
    operation character varying(32) NOT NULL,
    payload_json jsonb NOT NULL,
    occurred_at_utc timestamp with time zone NOT NULL,
    device_id uuid,
    user_id uuid,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_change_log_change_version_positive CHECK ((change_version > 0)),
    CONSTRAINT ck_change_log_operation CHECK (((operation)::text = ANY ((ARRAY['Upsert'::character varying, 'Deactivate'::character varying])::text[])))
);


--
-- Name: customers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.customers (
    id uuid NOT NULL,
    code character varying(64) NOT NULL,
    name character varying(256) NOT NULL,
    contact_name character varying(256),
    email character varying(256),
    phone character varying(64),
    notes character varying(512),
    is_active boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: devices; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.devices (
    id uuid NOT NULL,
    device_key character varying(128) NOT NULL,
    user_id uuid,
    name character varying(256) NOT NULL,
    status character varying(32) NOT NULL,
    application_version character varying(64),
    last_successful_sync_at_utc timestamp with time zone,
    last_successful_online_authentication_at_utc timestamp with time zone,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL,
    warehouse_id uuid,
    CONSTRAINT ck_devices_status CHECK (((status)::text = ANY ((ARRAY['Active'::character varying, 'Revoked'::character varying])::text[])))
);


--
-- Name: permissions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.permissions (
    id uuid NOT NULL,
    code character varying(128) NOT NULL,
    name character varying(256) NOT NULL,
    description character varying(512)
);


--
-- Name: plans; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.plans (
    id uuid NOT NULL,
    code character varying(64) NOT NULL,
    name character varying(256) NOT NULL,
    is_active boolean NOT NULL,
    max_users integer,
    max_warehouses integer,
    max_devices integer,
    max_products integer,
    feature_codes character varying(64)[] NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL
);


--
-- Name: platform_operators; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.platform_operators (
    id uuid NOT NULL,
    email character varying(256) NOT NULL,
    password_hash character varying(512) NOT NULL,
    display_name character varying(256) NOT NULL,
    is_active boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL
);


--
-- Name: pos_inventory_conflicts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pos_inventory_conflicts (
    id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    product_id uuid NOT NULL,
    source_command_id uuid,
    pos_sale_id uuid,
    resulting_on_hand numeric(18,6) NOT NULL,
    message character varying(1024) NOT NULL,
    status character varying(32) NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    resolved_at_utc timestamp with time zone,
    tenant_id uuid NOT NULL
);


--
-- Name: pos_payments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pos_payments (
    id uuid NOT NULL,
    pos_sale_id uuid NOT NULL,
    method character varying(32) NOT NULL,
    amount numeric(19,4) NOT NULL,
    status character varying(32) NOT NULL,
    reference character varying(128),
    tenant_id uuid NOT NULL
);


--
-- Name: pos_sale_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pos_sale_lines (
    id uuid NOT NULL,
    pos_sale_id uuid NOT NULL,
    product_id uuid NOT NULL,
    sku_snapshot character varying(64) NOT NULL,
    name_snapshot character varying(256) NOT NULL,
    quantity numeric(18,6) NOT NULL,
    unit_price numeric(19,4) NOT NULL,
    discount_amount numeric(19,4) NOT NULL,
    tax_amount numeric(19,4) NOT NULL,
    line_total numeric(19,4) NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: pos_sales; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pos_sales (
    id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    device_id uuid,
    cashier_user_id uuid,
    customer_id uuid,
    cash_shift_id uuid,
    client_command_id uuid NOT NULL,
    receipt_number character varying(64) NOT NULL,
    status character varying(32) NOT NULL,
    currency_code character varying(3) NOT NULL,
    subtotal numeric(19,4) NOT NULL,
    discount_total numeric(19,4) NOT NULL,
    tax_total numeric(19,4) NOT NULL,
    grand_total numeric(19,4) NOT NULL,
    sold_at_utc timestamp with time zone NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    voided_at_utc timestamp with time zone,
    notes character varying(1024),
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: price_lists; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.price_lists (
    id uuid NOT NULL,
    code character varying(32) NOT NULL,
    name character varying(256) NOT NULL,
    currency_code character varying(3) NOT NULL,
    is_active boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: processed_sync_operations; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.processed_sync_operations (
    id uuid NOT NULL,
    device_id uuid NOT NULL,
    user_id uuid NOT NULL,
    entity_type character varying(128) NOT NULL,
    entity_id uuid NOT NULL,
    operation_type character varying(128) NOT NULL,
    status character varying(32) NOT NULL,
    result_payload_json jsonb NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    processed_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_processed_sync_operations_status CHECK (((status)::text = ANY ((ARRAY['Succeeded'::character varying, 'Rejected'::character varying])::text[])))
);


--
-- Name: product_barcodes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.product_barcodes (
    id uuid NOT NULL,
    product_id uuid NOT NULL,
    barcode character varying(128) NOT NULL,
    is_primary boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: product_categories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.product_categories (
    id uuid NOT NULL,
    code character varying(64) NOT NULL,
    name character varying(256) NOT NULL,
    parent_category_id uuid,
    is_active boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: product_prices; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.product_prices (
    id uuid NOT NULL,
    price_list_id uuid NOT NULL,
    product_id uuid NOT NULL,
    unit_price numeric(19,4) NOT NULL,
    min_quantity numeric(18,6),
    tenant_id uuid NOT NULL
);


--
-- Name: products; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.products (
    id uuid NOT NULL,
    sku character varying(64) NOT NULL,
    name character varying(256) NOT NULL,
    description character varying(2000),
    brand character varying(128),
    category_id uuid,
    unit_of_measure_id uuid NOT NULL,
    purchase_price numeric(19,4) NOT NULL,
    sale_price numeric(19,4) NOT NULL,
    is_taxable boolean NOT NULL,
    is_active boolean NOT NULL,
    track_inventory boolean NOT NULL,
    minimum_stock numeric(18,6),
    maximum_stock numeric(18,6),
    reorder_point numeric(18,6),
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_products_maximum_stock_non_negative CHECK (((maximum_stock IS NULL) OR (maximum_stock >= (0)::numeric))),
    CONSTRAINT ck_products_minimum_stock_non_negative CHECK (((minimum_stock IS NULL) OR (minimum_stock >= (0)::numeric))),
    CONSTRAINT ck_products_purchase_price_non_negative CHECK ((purchase_price >= (0)::numeric)),
    CONSTRAINT ck_products_reorder_point_non_negative CHECK (((reorder_point IS NULL) OR (reorder_point >= (0)::numeric))),
    CONSTRAINT ck_products_sale_price_non_negative CHECK ((sale_price >= (0)::numeric))
);


--
-- Name: purchase_operations; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.purchase_operations (
    id uuid NOT NULL,
    purchase_order_id uuid NOT NULL,
    action character varying(32) NOT NULL,
    result_payload_json jsonb NOT NULL,
    processed_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: purchase_order_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.purchase_order_lines (
    id uuid NOT NULL,
    purchase_order_id uuid NOT NULL,
    product_id uuid NOT NULL,
    quantity_ordered numeric(18,6) NOT NULL,
    quantity_received numeric(18,6) NOT NULL,
    quantity_returned numeric(18,6) NOT NULL,
    unit_price numeric(19,4) NOT NULL,
    tax_rate numeric(9,6) NOT NULL,
    line_net numeric(19,4) NOT NULL,
    line_tax numeric(19,4) NOT NULL,
    line_total numeric(19,4) NOT NULL,
    notes character varying(512),
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_purchase_order_lines_qty_ordered_positive CHECK ((quantity_ordered > (0)::numeric)),
    CONSTRAINT ck_purchase_order_lines_qty_received_non_negative CHECK ((quantity_received >= (0)::numeric)),
    CONSTRAINT ck_purchase_order_lines_qty_returned_non_negative CHECK ((quantity_returned >= (0)::numeric)),
    CONSTRAINT ck_purchase_order_lines_received_not_over_ordered CHECK ((quantity_received <= quantity_ordered)),
    CONSTRAINT ck_purchase_order_lines_returned_not_over_received CHECK ((quantity_returned <= quantity_received)),
    CONSTRAINT ck_purchase_order_lines_tax_rate_non_negative CHECK ((tax_rate >= (0)::numeric)),
    CONSTRAINT ck_purchase_order_lines_unit_price_non_negative CHECK ((unit_price >= (0)::numeric))
);


--
-- Name: purchase_orders; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.purchase_orders (
    id uuid NOT NULL,
    supplier_id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    status character varying(32) NOT NULL,
    payment_status character varying(32) NOT NULL,
    reference character varying(128),
    notes character varying(512),
    subtotal numeric(19,4) NOT NULL,
    tax_total numeric(19,4) NOT NULL,
    grand_total numeric(19,4) NOT NULL,
    paid_amount numeric(19,4) NOT NULL,
    created_by_user_id uuid,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    confirmed_at_utc timestamp with time zone,
    closed_at_utc timestamp with time zone,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_purchase_orders_paid_non_negative CHECK ((paid_amount >= (0)::numeric)),
    CONSTRAINT ck_purchase_orders_payment_status CHECK (((payment_status)::text = ANY ((ARRAY['Unpaid'::character varying, 'PartiallyPaid'::character varying, 'Paid'::character varying])::text[]))),
    CONSTRAINT ck_purchase_orders_status CHECK (((status)::text = ANY ((ARRAY['Draft'::character varying, 'Confirmed'::character varying, 'PartiallyReceived'::character varying, 'FullyReceived'::character varying, 'Closed'::character varying, 'Cancelled'::character varying])::text[]))),
    CONSTRAINT ck_purchase_orders_totals_non_negative CHECK (((subtotal >= (0)::numeric) AND (tax_total >= (0)::numeric) AND (grand_total >= (0)::numeric)))
);


--
-- Name: purchase_receipt_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.purchase_receipt_lines (
    id uuid NOT NULL,
    purchase_receipt_id uuid NOT NULL,
    purchase_order_line_id uuid NOT NULL,
    product_id uuid NOT NULL,
    quantity numeric(18,6) NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_purchase_receipt_lines_quantity_positive CHECK ((quantity > (0)::numeric))
);


--
-- Name: purchase_receipts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.purchase_receipts (
    id uuid NOT NULL,
    purchase_order_id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    notes character varying(512),
    created_by_user_id uuid,
    operation_id uuid,
    received_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: purchase_return_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.purchase_return_lines (
    id uuid NOT NULL,
    purchase_return_id uuid NOT NULL,
    purchase_order_line_id uuid NOT NULL,
    product_id uuid NOT NULL,
    quantity numeric(18,6) NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_purchase_return_lines_quantity_positive CHECK ((quantity > (0)::numeric))
);


--
-- Name: purchase_returns; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.purchase_returns (
    id uuid NOT NULL,
    purchase_order_id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    notes character varying(512),
    created_by_user_id uuid,
    operation_id uuid,
    returned_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: refresh_tokens; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.refresh_tokens (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    device_id uuid,
    family_id uuid NOT NULL,
    token_hash character varying(64) NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    expires_at_utc timestamp with time zone NOT NULL,
    revoked_at_utc timestamp with time zone,
    replaced_by_token_id uuid,
    tenant_id uuid NOT NULL
);


--
-- Name: restaurant_open_orders; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.restaurant_open_orders (
    id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    table_id uuid NOT NULL,
    status character varying(32) NOT NULL,
    notes character varying(1024),
    opened_at_utc timestamp with time zone NOT NULL,
    closed_at_utc timestamp with time zone,
    pos_sale_id uuid,
    tenant_id uuid NOT NULL
);


--
-- Name: restaurant_tables; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.restaurant_tables (
    id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    code character varying(32) NOT NULL,
    name character varying(128) NOT NULL,
    is_active boolean NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: role_permissions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.role_permissions (
    id uuid NOT NULL,
    role_id uuid NOT NULL,
    permission_id uuid NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: roles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.roles (
    id uuid NOT NULL,
    code character varying(64) NOT NULL,
    name character varying(128) NOT NULL,
    is_system boolean NOT NULL,
    is_active boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: sales_operations; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_operations (
    id uuid NOT NULL,
    sales_order_id uuid NOT NULL,
    action character varying(32) NOT NULL,
    result_payload_json jsonb NOT NULL,
    processed_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: sales_order_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_order_lines (
    id uuid NOT NULL,
    sales_order_id uuid NOT NULL,
    product_id uuid NOT NULL,
    quantity_ordered numeric(18,6) NOT NULL,
    quantity_shipped numeric(18,6) NOT NULL,
    quantity_returned numeric(18,6) NOT NULL,
    unit_price numeric(19,4) NOT NULL,
    tax_rate numeric(9,6) NOT NULL,
    line_net numeric(19,4) NOT NULL,
    line_tax numeric(19,4) NOT NULL,
    line_total numeric(19,4) NOT NULL,
    notes character varying(512),
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_sales_order_lines_qty_ordered_positive CHECK ((quantity_ordered > (0)::numeric)),
    CONSTRAINT ck_sales_order_lines_qty_returned_non_negative CHECK ((quantity_returned >= (0)::numeric)),
    CONSTRAINT ck_sales_order_lines_qty_shipped_non_negative CHECK ((quantity_shipped >= (0)::numeric)),
    CONSTRAINT ck_sales_order_lines_returned_not_over_shipped CHECK ((quantity_returned <= quantity_shipped)),
    CONSTRAINT ck_sales_order_lines_shipped_not_over_ordered CHECK ((quantity_shipped <= quantity_ordered)),
    CONSTRAINT ck_sales_order_lines_tax_rate_non_negative CHECK ((tax_rate >= (0)::numeric)),
    CONSTRAINT ck_sales_order_lines_unit_price_non_negative CHECK ((unit_price >= (0)::numeric))
);


--
-- Name: sales_orders; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_orders (
    id uuid NOT NULL,
    customer_id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    status character varying(32) NOT NULL,
    payment_status character varying(32) NOT NULL,
    reference character varying(128),
    notes character varying(512),
    subtotal numeric(19,4) NOT NULL,
    tax_total numeric(19,4) NOT NULL,
    grand_total numeric(19,4) NOT NULL,
    paid_amount numeric(19,4) NOT NULL,
    created_by_user_id uuid,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    confirmed_at_utc timestamp with time zone,
    closed_at_utc timestamp with time zone,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_sales_orders_paid_non_negative CHECK ((paid_amount >= (0)::numeric)),
    CONSTRAINT ck_sales_orders_payment_status CHECK (((payment_status)::text = ANY ((ARRAY['Unpaid'::character varying, 'PartiallyPaid'::character varying, 'Paid'::character varying])::text[]))),
    CONSTRAINT ck_sales_orders_status CHECK (((status)::text = ANY ((ARRAY['Draft'::character varying, 'Confirmed'::character varying, 'PartiallyFulfilled'::character varying, 'Fulfilled'::character varying, 'Closed'::character varying, 'Cancelled'::character varying])::text[]))),
    CONSTRAINT ck_sales_orders_totals_non_negative CHECK (((subtotal >= (0)::numeric) AND (tax_total >= (0)::numeric) AND (grand_total >= (0)::numeric)))
);


--
-- Name: sales_return_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_return_lines (
    id uuid NOT NULL,
    sales_return_id uuid NOT NULL,
    sales_order_line_id uuid NOT NULL,
    product_id uuid NOT NULL,
    quantity numeric(18,6) NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_sales_return_lines_quantity_positive CHECK ((quantity > (0)::numeric))
);


--
-- Name: sales_returns; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_returns (
    id uuid NOT NULL,
    sales_order_id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    notes character varying(512),
    created_by_user_id uuid,
    operation_id uuid,
    returned_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: sales_shipment_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_shipment_lines (
    id uuid NOT NULL,
    sales_shipment_id uuid NOT NULL,
    sales_order_line_id uuid NOT NULL,
    product_id uuid NOT NULL,
    quantity numeric(18,6) NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_sales_shipment_lines_quantity_positive CHECK ((quantity > (0)::numeric))
);


--
-- Name: sales_shipments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_shipments (
    id uuid NOT NULL,
    sales_order_id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    notes character varying(512),
    created_by_user_id uuid,
    operation_id uuid,
    shipped_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: stock_adjustment_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.stock_adjustment_lines (
    id uuid NOT NULL,
    stock_adjustment_id uuid NOT NULL,
    product_id uuid NOT NULL,
    location_id uuid NOT NULL,
    quantity_change numeric(18,6) NOT NULL,
    notes character varying(512),
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_stock_adjustment_lines_quantity_change_nonzero CHECK ((quantity_change <> (0)::numeric))
);


--
-- Name: stock_adjustments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.stock_adjustments (
    id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    kind character varying(32) NOT NULL,
    status character varying(32) NOT NULL,
    reason character varying(512),
    created_by_user_id uuid,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    posted_at_utc timestamp with time zone,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_stock_adjustments_kind CHECK (((kind)::text = ANY ((ARRAY['OpeningBalance'::character varying, 'Correction'::character varying])::text[]))),
    CONSTRAINT ck_stock_adjustments_status CHECK (((status)::text = ANY ((ARRAY['Draft'::character varying, 'Approved'::character varying, 'Posted'::character varying, 'Cancelled'::character varying])::text[])))
);


--
-- Name: stock_balances; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.stock_balances (
    id uuid NOT NULL,
    product_id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    on_hand numeric(18,6) NOT NULL,
    reserved numeric(18,6) NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: stock_count_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.stock_count_lines (
    id uuid NOT NULL,
    stock_count_id uuid NOT NULL,
    product_id uuid NOT NULL,
    system_quantity numeric(18,6) NOT NULL,
    counted_quantity numeric(18,6),
    notes character varying(512),
    tenant_id uuid NOT NULL
);


--
-- Name: stock_counts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.stock_counts (
    id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    status character varying(32) NOT NULL,
    notes character varying(512),
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    posted_at_utc timestamp with time zone,
    posted_adjustment_id uuid,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_stock_counts_status CHECK (((status)::text = ANY ((ARRAY['Draft'::character varying, 'Counted'::character varying, 'Posted'::character varying, 'Cancelled'::character varying])::text[])))
);


--
-- Name: stock_movements; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.stock_movements (
    id uuid NOT NULL,
    product_id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    location_id uuid NOT NULL,
    quantity_change numeric(18,6) NOT NULL,
    movement_type character varying(32) NOT NULL,
    source_document_type character varying(64) NOT NULL,
    source_document_id uuid,
    transfer_line_id uuid,
    created_by_user_id uuid,
    notes character varying(512),
    occurred_at_utc timestamp with time zone NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: suppliers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.suppliers (
    id uuid NOT NULL,
    code character varying(64) NOT NULL,
    name character varying(256) NOT NULL,
    contact_name character varying(256),
    email character varying(256),
    phone character varying(64),
    notes character varying(512),
    is_active boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: tenant_change_counters; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.tenant_change_counters (
    tenant_id uuid NOT NULL,
    last_change_version bigint NOT NULL
);


--
-- Name: tenant_subscriptions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.tenant_subscriptions (
    id uuid NOT NULL,
    plan_id uuid NOT NULL,
    status character varying(32) NOT NULL,
    trial_ends_at_utc timestamp with time zone,
    current_period_start_utc timestamp with time zone,
    current_period_end_utc timestamp with time zone,
    external_reference character varying(256),
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_tenant_subscriptions_status CHECK (((status)::text = ANY ((ARRAY['Trial'::character varying, 'Active'::character varying, 'PastDue'::character varying, 'Cancelled'::character varying])::text[])))
);


--
-- Name: tenants; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.tenants (
    id uuid NOT NULL,
    code character varying(64) NOT NULL,
    name character varying(256) NOT NULL,
    is_active boolean NOT NULL,
    is_suspended boolean NOT NULL,
    currency_code character varying(3) NOT NULL,
    time_zone_id character varying(64) NOT NULL,
    negative_stock_policy character varying(32) NOT NULL,
    tax_enabled boolean NOT NULL,
    tax_name character varying(128),
    tax_rate numeric(9,6),
    max_offline_days integer,
    require_online_revalidation boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    CONSTRAINT ck_tenants_currency_code_length CHECK ((char_length((currency_code)::text) = 3)),
    CONSTRAINT ck_tenants_max_offline_days_non_negative CHECK (((max_offline_days IS NULL) OR (max_offline_days >= 0))),
    CONSTRAINT ck_tenants_negative_stock_policy CHECK (((negative_stock_policy)::text = ANY ((ARRAY['Prevent'::character varying, 'Allow'::character varying, 'AllowWithPermission'::character varying])::text[]))),
    CONSTRAINT ck_tenants_tax_configuration CHECK ((((tax_enabled = false) AND (tax_name IS NULL) AND (tax_rate IS NULL)) OR ((tax_enabled = true) AND (tax_name IS NOT NULL) AND (tax_rate IS NOT NULL)))),
    CONSTRAINT ck_tenants_tax_rate_non_negative CHECK (((tax_rate IS NULL) OR (tax_rate >= (0)::numeric)))
);


--
-- Name: transfer_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.transfer_lines (
    id uuid NOT NULL,
    transfer_id uuid NOT NULL,
    product_id uuid NOT NULL,
    quantity_requested numeric(18,6) NOT NULL,
    quantity_received numeric(18,6) NOT NULL,
    notes character varying(512),
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_transfer_lines_quantity_received_non_negative CHECK ((quantity_received >= (0)::numeric)),
    CONSTRAINT ck_transfer_lines_quantity_requested_positive CHECK ((quantity_requested > (0)::numeric)),
    CONSTRAINT ck_transfer_lines_received_not_over_requested CHECK ((quantity_received <= quantity_requested))
);


--
-- Name: transfer_operations; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.transfer_operations (
    id uuid NOT NULL,
    transfer_id uuid NOT NULL,
    action character varying(32) NOT NULL,
    result_payload_json jsonb NOT NULL,
    processed_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: transfers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.transfers (
    id uuid NOT NULL,
    source_warehouse_id uuid NOT NULL,
    destination_warehouse_id uuid NOT NULL,
    source_location_id uuid NOT NULL,
    destination_location_id uuid NOT NULL,
    status character varying(32) NOT NULL,
    notes character varying(512),
    created_by_user_id uuid,
    dispatch_operation_id uuid,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    dispatched_at_utc timestamp with time zone,
    completed_at_utc timestamp with time zone,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_transfers_status CHECK (((status)::text = ANY ((ARRAY['Draft'::character varying, 'Dispatched'::character varying, 'PartiallyReceived'::character varying, 'Completed'::character varying, 'Cancelled'::character varying])::text[]))),
    CONSTRAINT ck_transfers_warehouses_differ CHECK ((source_warehouse_id <> destination_warehouse_id))
);


--
-- Name: units_of_measure; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.units_of_measure (
    id uuid NOT NULL,
    code character varying(32) NOT NULL,
    name character varying(128) NOT NULL,
    is_system boolean NOT NULL,
    is_active boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: user_roles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_roles (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    role_id uuid NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.users (
    id uuid NOT NULL,
    email character varying(320) NOT NULL,
    password_hash character varying(512) NOT NULL,
    display_name character varying(256) NOT NULL,
    is_active boolean NOT NULL,
    is_disabled boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Name: warehouse_locations; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.warehouse_locations (
    id uuid NOT NULL,
    warehouse_id uuid NOT NULL,
    code character varying(64) NOT NULL,
    name character varying(256) NOT NULL,
    kind character varying(32) NOT NULL,
    is_default boolean NOT NULL,
    is_active boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL,
    CONSTRAINT ck_warehouse_locations_in_transit_not_default CHECK ((((kind)::text <> 'InTransit'::text) OR (is_default = false))),
    CONSTRAINT ck_warehouse_locations_kind CHECK (((kind)::text = ANY ((ARRAY['Normal'::character varying, 'System'::character varying, 'InTransit'::character varying])::text[])))
);


--
-- Name: warehouses; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.warehouses (
    id uuid NOT NULL,
    code character varying(64) NOT NULL,
    name character varying(256) NOT NULL,
    is_active boolean NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone NOT NULL,
    row_version integer NOT NULL,
    tenant_id uuid NOT NULL
);


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" (migration_id, product_version) FROM stdin;
20260926141912_InitialDatabaseFoundation	8.0.11
20260926205000_AddRefreshTokens	8.0.11
20260926210000_AddProductCatalog	8.0.11
20260926213000_AddInventory	8.0.11
20260927120000_AddTransfers	8.0.11
20260927140000_AddPurchasing	8.0.11
20260927160000_AddSales	8.0.11
20260927180000_AddSyncChangeLog	8.0.11
20260927203518_AddSaaSPlatform	8.0.11
20260929082527_AddStockCounts	8.0.11
20261026105057_AddPosModule	8.0.11
\.


--
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.audit_logs (id, tenant_id, user_id, device_id, action, entity_type, entity_id, occurred_at_utc, correlation_id, ip_address, metadata_json) FROM stdin;
8b183300-448f-4643-91ac-159c786498d5	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:09:31.586144+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
f544976e-4b4b-48e1-87fd-af9792995e65	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:09:57.080573+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
1a1200e6-7aef-4f50-a5d6-36d7b1554d7a	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:09:57.369391+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
0aedbfca-e384-4144-a7d5-95d4954afec2	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:10:05.909662+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
39f86d7f-0842-4292-b87f-82a5af6ce807	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:10:05.9305+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
8bd1b8c3-b824-4ae0-bea4-4ba8e6305cfc	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:10:10.817524+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
8a1252dc-48eb-4c8a-a0c3-1e607205f599	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:10:10.838178+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
e4d1107d-93c1-48fe-beaa-9a84b1c7a3ee	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.tenant.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-09-27 23:10:14.6137+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
39837573-2278-4422-ac4d-4123f237855e	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.tenant.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-09-27 23:10:14.648342+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
7eb17575-302c-4993-a32b-27b78fcabe43	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.readiness.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-09-27 23:10:14.690452+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
42e1ace1-4d90-4df1-b7ad-55413e79bf4b	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.subscription.viewed	TenantSubscription	8e82b609-ea20-4279-aa18-947583dc9e8b	2026-09-27 23:10:14.710818+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
bcfaae6d-36ae-4f1d-9c5d-b60d65e87266	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.readiness.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-09-27 23:10:14.731632+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
cb2b2a57-8447-4340-bcfe-acd32b84d4c6	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.subscription.viewed	TenantSubscription	8e82b609-ea20-4279-aa18-947583dc9e8b	2026-09-27 23:10:14.754524+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
c90f6a48-30c0-447a-9eff-0fc988c4da56	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.usage.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-09-27 23:10:14.823215+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
e565be85-09ea-4eda-b936-1e03aa5f6dc0	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.usage.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-09-27 23:10:14.870015+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
fe7027de-fda2-45b3-9cc6-c3441533297c	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:10:43.841925+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
0feae8d7-da5b-401a-804e-6293149d0d83	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:10:43.872614+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
07461825-bd74-4995-97c4-f01c24ef1e72	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.tenant.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:10:45.881592+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
d0eeaf0b-501c-42e5-97ea-64a24ebdcee4	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.subscription.viewed	TenantSubscription	df9191a8-d089-457a-b05e-351b953959bd	2026-09-27 23:10:45.894867+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
860dfc93-fc3b-4499-a635-a8acee96aeb7	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.usage.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:10:45.899045+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
638a9ce2-5bed-4dda-bf89-54720814489b	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.readiness.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:10:45.901939+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
5a3f037b-1395-4e54-ae5e-93c778e285bd	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.tenant.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:10:45.921761+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
ef6543d7-82e1-4a7b-b51d-08efe7a041f4	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.subscription.viewed	TenantSubscription	df9191a8-d089-457a-b05e-351b953959bd	2026-09-27 23:10:45.933059+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
49bec8f6-e519-4862-86ec-94a8e38870db	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.readiness.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:10:45.952928+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
34a7202b-2e8a-485a-8d8a-9a2094bd8da7	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.usage.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:10:45.957161+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
6ae89e81-8356-4987-8e8e-e0067d1a37c1	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:12:09.852635+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
766b2a93-272b-4b2d-aef0-f56c3bae959e	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.tenant.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:12:22.766728+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
30fa8327-930e-4ff2-9274-a8b4f7110b7d	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.usage.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:12:22.797875+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
2ef59b06-cc7b-496c-95c9-cdddfbf50dc3	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.subscription.viewed	TenantSubscription	df9191a8-d089-457a-b05e-351b953959bd	2026-09-27 23:12:22.841369+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
a00e2710-3509-4620-bd6b-1ce25f150f7f	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-27 23:12:09.877001+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
e5ea21f1-3630-4903-a3bc-c9d5b23731ec	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.tenant.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:12:22.821796+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
7b69e0e1-74ed-42a6-8323-e92e64245cec	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.readiness.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:12:22.84697+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
2f63a145-c0d8-46f1-92c4-d27aebd3a4eb	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.subscription.viewed	TenantSubscription	df9191a8-d089-457a-b05e-351b953959bd	2026-09-27 23:12:22.789861+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
be7e690c-9fb9-45b9-9537-cbb86f639278	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.readiness.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:12:22.793257+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
f440286a-9800-41a2-b501-fdcb131f9afa	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N	\N	platform.usage.viewed	Tenant	e534cf35-c3f3-47e1-92a4-ed14befb3e91	2026-09-27 23:12:22.856617+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
ca1eada7-2cb8-4734-86b5-f85e2e6ebbd3	e534cf35-c3f3-47e1-92a4-ed14befb3e91	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	auth.login	User	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-27 23:12:47.960424+02	\N	\N	\N
e6d32c69-f2e7-4f91-ac82-5dbe661825ed	e534cf35-c3f3-47e1-92a4-ed14befb3e91	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	auth.login	User	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-27 23:53:32.975747+02	\N	\N	\N
cd15bc0c-3d8a-44f9-b8a6-6edde4d20e35	e534cf35-c3f3-47e1-92a4-ed14befb3e91	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	auth.login	User	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:14:30.710503+02	\N	\N	\N
04e4c29c-6d79-465a-8057-9a294d53f317	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-29 11:02:15.013007+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
0ab82fbb-679b-4f42-8244-68d5c8ebddd8	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-29 11:02:15.359545+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
ba4c79c5-307f-42d8-be36-951f4eb34c2e	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-29 11:02:20.752077+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
8cd81ac3-7708-4676-98a0-7b579ead51de	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-29 11:02:20.777119+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
3fabaa2e-71f6-4a4d-8bc8-93af4777b41c	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-29 11:02:24.11391+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
7cc5a540-2146-4f29-bacb-f6721ec8ba0c	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-09-29 11:02:24.136109+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
50213823-712f-48d1-a421-e4e0f177b6d7	e534cf35-c3f3-47e1-92a4-ed14befb3e91	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	auth.login	User	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-29 11:02:41.654072+02	\N	\N	\N
02b6c538-daa8-47ac-8f9b-fec6f1ed751d	e534cf35-c3f3-47e1-92a4-ed14befb3e91	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	auth.login	User	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-29 12:35:23.270433+02	\N	\N	\N
3ed2d161-08bb-45ba-82ea-db73d209acac	e534cf35-c3f3-47e1-92a4-ed14befb3e91	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	auth.login	User	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-29 23:17:55.966384+02	\N	\N	\N
07430c45-ca07-46bd-873f-076b7c1fd42b	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-10-26 13:15:05.088279+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
7939da1f-2ea9-40ad-ad50-bb42837c777e	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	782ad8a3-5bb9-45f3-9f58-2da06c249a08	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:15:48.77465+02	\N	\N	\N
462f43ed-fc2d-41b2-bf63-ffe9092d2ee6	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:17:23.922271+02	\N	\N	\N
fdb866bb-62f2-4a8b-8af7-6c9287b923b7	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:24:23.952261+02	\N	\N	\N
2c4bab2d-ec34-4e66-a8ff-df4188599c23	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:25:46.500339+02	\N	\N	\N
bc458d86-d124-445e-9baf-c4930d1e6435	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:26:24.850574+02	\N	\N	\N
fd878364-d2fa-4458-b5db-b13472521949	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:27:08.49961+02	\N	\N	\N
b06c7298-ab0f-42f9-9f93-0aa3b50b9b08	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 21:40:21.916653+02	\N	\N	\N
8407cccc-eb38-4f3d-bc04-47b8a83eb5ef	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-10-27 11:08:08.547933+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
c5921e23-de30-4e0c-90a9-0187433debe0	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-10-27 11:08:08.577241+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
60fe0138-8be0-4627-83ed-b7a175e1b0e6	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-10-27 11:09:07.975331+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
93800b35-fc60-4049-8732-4fafd619ba2e	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-10-27 11:09:07.991608+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
e5c92faf-4275-471b-b772-ebd55021c09a	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-10-27 11:09:15.145993+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
0239a02d-bcad-4f77-b616-23d9ba3306bd	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-10-27 11:09:15.168174+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
6d1e33d9-14e6-47ca-8629-b14ba282b808	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.tenant.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-10-27 11:09:24.870736+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
4df4a1e7-b74c-4820-8c49-b1963eb29908	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.tenant.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-10-27 11:09:24.896005+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
65081bcd-0e2f-4e53-ade3-ef30999e068f	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.subscription.viewed	TenantSubscription	8e82b609-ea20-4279-aa18-947583dc9e8b	2026-10-27 11:09:24.983473+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
2ff95991-f624-4a48-9869-c87ef856fe0b	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.readiness.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-10-27 11:09:25.008577+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
6e2174cc-0f8c-43b5-a393-ffbdb136a20f	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.usage.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-10-27 11:09:25.009381+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
da1bb1df-5f77-4aad-a167-f572671d5009	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.subscription.viewed	TenantSubscription	8e82b609-ea20-4279-aa18-947583dc9e8b	2026-10-27 11:09:25.011595+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
cefc3e70-4985-467c-b73c-5aab0de4a94d	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.readiness.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-10-27 11:09:25.039137+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
18226974-5b3c-407f-bed4-5090cba7413e	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N	\N	platform.usage.viewed	Tenant	bd9cbe72-fe4c-4753-be04-d3040b554f67	2026-10-27 11:09:25.047274+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
6db8eaa8-ba89-4efc-9dbd-6735bd063cdd	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-10-27 11:10:02.907826+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
01159f2c-5450-47f4-82a3-3cc318a8dd34	\N	\N	\N	platform.tenants.listed	Tenant	\N	2026-10-27 11:10:02.921052+02	\N	\N	{"platformOperatorId": "a955398a-a624-4f42-be87-d1f990126b3d", "platformOperatorEmail": "platform@warehouse.local"}
57f7ab92-faaa-41c1-a9b1-10794cab0093	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-27 11:13:29.739238+02	\N	\N	\N
75c65f17-653d-4bb1-a006-1482cfe9f6cf	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 10:29:01.536347+02	\N	\N	\N
e5c5af68-90fa-48fb-9c0d-bd1913834227	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 10:58:21.398801+02	\N	\N	\N
c364fecf-2bf1-4cb2-9826-b8378a525cea	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 11:15:18.964012+02	\N	\N	\N
66a23108-ea4f-4eb3-b616-3b103908b988	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 11:33:26.637772+02	\N	\N	\N
f4e36cfb-5ceb-4107-a996-7360dec3ad60	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 11:55:57.972602+02	\N	\N	\N
9cdf701b-7250-4d95-81ae-47787cd2d7bf	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 12:45:59.554535+02	\N	\N	\N
ee5061b8-d569-42b2-af37-b69a8b870c71	bd9cbe72-fe4c-4753-be04-d3040b554f67	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	auth.login	User	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-30 12:13:45.638438+02	\N	\N	\N
\.


--
-- Data for Name: cash_movements; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.cash_movements (id, cash_shift_id, type, amount, reason, created_at_utc, tenant_id) FROM stdin;
\.


--
-- Data for Name: cash_shifts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.cash_shifts (id, warehouse_id, device_id, cashier_user_id, status, opening_float, closing_counted_cash, opened_at_utc, closed_at_utc, row_version, tenant_id) FROM stdin;
80c34921-765b-4c31-9a12-9429fb69db7c	bd357264-c8d1-472b-987d-575ac34ff384	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Open	0.0000	\N	2026-10-26 13:27:16.822656+02	\N	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: change_log; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.change_log (id, change_version, entity_type, entity_id, operation, payload_json, occurred_at_utc, device_id, user_id, tenant_id) FROM stdin;
\.


--
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.customers (id, code, name, contact_name, email, phone, notes, is_active, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
8e6f8326-582f-44cf-b799-500e3e95722b	15	???? ????	\N	\N	\N	\N	t	2026-09-27 23:57:52.20619+02	2026-09-27 23:57:52.20619+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
2be50852-dade-4c13-b6c7-f4c08a148594	12	marker	\N	\N	\N	\N	t	2026-10-26 13:22:53.342503+02	2026-10-26 13:22:53.342503+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: devices; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.devices (id, device_key, user_id, name, status, application_version, last_successful_sync_at_utc, last_successful_online_authentication_at_utc, created_at_utc, updated_at_utc, row_version, tenant_id, warehouse_id) FROM stdin;
7a37ba84-62ff-4d03-a15f-0c14ec5ae373	e80f2771-aa32-4939-a036-40a8d9437f45	813a8227-bc81-4580-ba4e-598cb2bf2231	Web	Active	0.1.0-web	2026-09-28 00:03:07.124045+02	2026-09-29 23:17:55.966384+02	2026-09-27 23:12:47.960424+02	2026-09-29 23:17:55.966384+02	7	e534cf35-c3f3-47e1-92a4-ed14befb3e91	\N
782ad8a3-5bb9-45f3-9f58-2da06c249a08	6cd5c21d-b75a-4f9a-b312-ea25e478e237	b3b30173-470c-49f7-9991-8f4e0dbea3de	Web	Active	0.1.0-web	\N	2026-10-26 13:15:48.77465+02	2026-10-26 13:15:48.77465+02	2026-10-26 13:15:48.77465+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N
3ccbcaf8-b061-4afd-9f79-c204669e8eb0	24ab7f51-140a-4b0d-a7c5-9395b0e62213	b3b30173-470c-49f7-9991-8f4e0dbea3de	Web	Active	0.1.0-web	2026-10-29 11:01:25.348265+02	2026-10-30 12:13:45.638438+02	2026-10-26 13:17:23.922271+02	2026-10-30 12:13:45.638438+02	17	bd9cbe72-fe4c-4753-be04-d3040b554f67	\N
\.


--
-- Data for Name: permissions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.permissions (id, code, name, description) FROM stdin;
11111111-1111-1111-1111-111111111101	products.read	Read products	\N
11111111-1111-1111-1111-111111111102	products.create	Create products	\N
11111111-1111-1111-1111-111111111103	products.update	Update products	\N
11111111-1111-1111-1111-111111111104	products.delete	Delete products	\N
11111111-1111-1111-1111-111111111105	inventory.read	Read inventory	\N
11111111-1111-1111-1111-111111111106	inventory.receive	Receive inventory	\N
11111111-1111-1111-1111-111111111107	inventory.issue	Issue inventory	\N
11111111-1111-1111-1111-111111111108	inventory.adjust	Adjust inventory	\N
11111111-1111-1111-1111-111111111109	inventory.transfer	Transfer inventory	\N
11111111-1111-1111-1111-111111111110	purchases.create	Create purchases	\N
11111111-1111-1111-1111-111111111111	purchases.approve	Approve purchases	\N
11111111-1111-1111-1111-111111111112	sales.create	Create sales	\N
11111111-1111-1111-1111-111111111113	sales.cancel	Cancel sales	\N
11111111-1111-1111-1111-111111111114	reports.read	Read reports	\N
11111111-1111-1111-1111-111111111115	users.manage	Manage users	\N
11111111-1111-1111-1111-111111111116	pos.sale	POS checkout	\N
11111111-1111-1111-1111-111111111117	pos.void	Void POS sales	\N
11111111-1111-1111-1111-111111111118	pos.refund	Refund POS sales	\N
11111111-1111-1111-1111-111111111119	pos.shift	POS cash shifts	\N
\.


--
-- Data for Name: plans; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.plans (id, code, name, is_active, max_users, max_warehouses, max_devices, max_products, feature_codes, created_at_utc, updated_at_utc) FROM stdin;
22222222-2222-2222-2222-222222222201	TRIAL	Trial	t	5	2	5	100	{catalog,inventory,purchasing,sales,transfers,sync,reports,offline}	2026-09-27 22:57:10.7235+02	2026-09-27 22:57:10.7235+02
22222222-2222-2222-2222-222222222202	STANDARD	Standard	t	50	20	50	10000	{catalog,inventory,purchasing,sales,transfers,sync,reports,offline}	2026-09-27 22:57:10.7235+02	2026-09-27 22:57:10.7235+02
22222222-2222-2222-2222-222222222203	ENTERPRISE	Enterprise	t	\N	\N	\N	\N	{catalog,inventory,purchasing,sales,transfers,sync,reports,offline}	2026-09-27 22:57:10.7235+02	2026-09-27 22:57:10.7235+02
\.


--
-- Data for Name: platform_operators; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.platform_operators (id, email, password_hash, display_name, is_active, created_at_utc, updated_at_utc) FROM stdin;
a955398a-a624-4f42-be87-d1f990126b3d	platform@warehouse.local	AQAAAAIAAYagAAAAED8BSvwRzJjAUCmFIHtxdk98aXA6DJ6jzALLkwpMHingPqgb2YYrqvyeL/WRhlMSqw==	Platform Admin	t	2026-09-27 22:57:12.263392+02	2026-09-27 22:57:12.263392+02
\.


--
-- Data for Name: pos_inventory_conflicts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.pos_inventory_conflicts (id, warehouse_id, product_id, source_command_id, pos_sale_id, resulting_on_hand, message, status, created_at_utc, resolved_at_utc, tenant_id) FROM stdin;
\.


--
-- Data for Name: pos_payments; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.pos_payments (id, pos_sale_id, method, amount, status, reference, tenant_id) FROM stdin;
1af7e4da-e39c-47e7-91a5-9e6ca92355c9	6f8360a8-4e05-401d-9c8f-e937c254bd14	Cash	60.0000	Captured	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
e6906c78-1df7-45ef-a333-667bfe8ee892	5aefd3bd-f483-4538-a290-a34c251d5f91	Cash	6.0000	Captured	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
e8e5c5f7-2ef4-4f58-8a0d-243269c7aabe	a8957e96-02a1-4b4d-b1bd-3d6a5d92e01a	Cash	6.0000	Captured	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
02733617-10de-44d5-b72d-3eb19159a85d	bb6c8fe8-4f2d-4ae3-8432-4cac9135a4ed	Cash	320.0000	Captured	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: pos_sale_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.pos_sale_lines (id, pos_sale_id, product_id, sku_snapshot, name_snapshot, quantity, unit_price, discount_amount, tax_amount, line_total, tenant_id) FROM stdin;
361a0299-be08-4180-91fb-633a6b509767	6f8360a8-4e05-401d-9c8f-e937c254bd14	0126728d-23b2-45bf-8065-cfd7455d4db0	12	kitkat	10.000000	6.0000	0.0000	0.0000	60.0000	bd9cbe72-fe4c-4753-be04-d3040b554f67
505ccc8e-6670-46df-a274-5e3a033bd3b4	5aefd3bd-f483-4538-a290-a34c251d5f91	0126728d-23b2-45bf-8065-cfd7455d4db0	12	kitkat	1.000000	6.0000	0.0000	0.0000	6.0000	bd9cbe72-fe4c-4753-be04-d3040b554f67
3f107f05-501e-4774-9e58-d028c1a682b3	a8957e96-02a1-4b4d-b1bd-3d6a5d92e01a	0126728d-23b2-45bf-8065-cfd7455d4db0	12	kitkat	1.000000	6.0000	0.0000	0.0000	6.0000	bd9cbe72-fe4c-4753-be04-d3040b554f67
e7affe11-ccc4-4fdc-bc20-1afc12fcff95	bb6c8fe8-4f2d-4ae3-8432-4cac9135a4ed	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	13	SSD Harddisk	2.000000	160.0000	0.0000	0.0000	320.0000	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: pos_sales; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.pos_sales (id, warehouse_id, location_id, device_id, cashier_user_id, customer_id, cash_shift_id, client_command_id, receipt_number, status, currency_code, subtotal, discount_total, tax_total, grand_total, sold_at_utc, created_at_utc, updated_at_utc, voided_at_utc, notes, row_version, tenant_id) FROM stdin;
6f8360a8-4e05-401d-9c8f-e937c254bd14	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	80c34921-765b-4c31-9a12-9429fb69db7c	6ba6e010-0196-4ee9-93bb-272eca09f605	POS-20261026-641D1F90	Completed	LYD	60.0000	0.0000	0.0000	60.0000	2026-10-26 13:27:37.859029+02	2026-10-26 13:27:37.859029+02	2026-10-26 13:27:37.859029+02	\N	\N	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
5aefd3bd-f483-4538-a290-a34c251d5f91	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	80c34921-765b-4c31-9a12-9429fb69db7c	0527303e-8b00-4890-ab5a-72bb134e906e	POS-20261026-9C6A7781	Completed	LYD	6.0000	0.0000	0.0000	6.0000	2026-10-26 13:28:55.772715+02	2026-10-26 13:28:55.772715+02	2026-10-26 13:28:55.772715+02	\N	\N	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
a8957e96-02a1-4b4d-b1bd-3d6a5d92e01a	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	80c34921-765b-4c31-9a12-9429fb69db7c	e970135f-5e72-4596-8aa7-1480f101eae2	POS-20261026-2CA2ABBC	Completed	LYD	6.0000	0.0000	0.0000	6.0000	2026-10-26 21:41:03.292007+02	2026-10-26 21:41:03.292007+02	2026-10-26 21:41:03.292007+02	\N	\N	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
bb6c8fe8-4f2d-4ae3-8432-4cac9135a4ed	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	80c34921-765b-4c31-9a12-9429fb69db7c	02f61c9a-01b5-4a3b-a511-0b4c0a18e88e	POS-20261029-C798DDFF	Completed	LYD	320.0000	0.0000	0.0000	320.0000	2026-10-29 12:55:44.700188+02	2026-10-29 12:55:44.700188+02	2026-10-29 12:55:44.700188+02	\N	\N	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: price_lists; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.price_lists (id, code, name, currency_code, is_active, created_at_utc, tenant_id) FROM stdin;
\.


--
-- Data for Name: processed_sync_operations; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.processed_sync_operations (id, device_id, user_id, entity_type, entity_id, operation_type, status, result_payload_json, created_at_utc, processed_at_utc, tenant_id) FROM stdin;
\.


--
-- Data for Name: product_barcodes; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.product_barcodes (id, product_id, barcode, is_primary, created_at_utc, tenant_id) FROM stdin;
838c7f46-c53a-4467-9c77-62ed327011e7	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	12	t	2026-09-27 23:55:47.446325+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
f18a6817-96f8-4528-a0a6-20a1f9c2dd05	0126728d-23b2-45bf-8065-cfd7455d4db0	12	t	2026-10-26 13:20:23.469808+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
ac53404c-ece1-4ac3-88c4-0c8b115da06c	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	13	t	2026-10-29 11:06:33.293298+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: product_categories; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.product_categories (id, code, name, parent_category_id, is_active, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
\.


--
-- Data for Name: product_prices; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.product_prices (id, price_list_id, product_id, unit_price, min_quantity, tenant_id) FROM stdin;
\.


--
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.products (id, sku, name, description, brand, category_id, unit_of_measure_id, purchase_price, sale_price, is_taxable, is_active, track_inventory, minimum_stock, maximum_stock, reorder_point, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	12	???	\N	\N	\N	c7ec4bd0-756d-4464-a669-e82e9aa83141	0.5000	0.7500	f	t	t	\N	\N	\N	2026-09-27 23:55:47.269032+02	2026-09-27 23:55:47.269032+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
0126728d-23b2-45bf-8065-cfd7455d4db0	12	kitkat	\N	\N	\N	ed090f18-3dfe-4a92-8b94-2e313adbd1ce	5.0000	6.0000	f	t	t	\N	\N	\N	2026-10-26 13:20:23.324006+02	2026-10-26 13:20:23.324006+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	13	SSD Harddisk	\N	\N	\N	e56bd4e4-01fa-4e13-90bc-b097cc3f01d6	150.0000	160.0000	f	t	t	\N	\N	\N	2026-10-29 11:06:33.201776+02	2026-10-29 11:06:33.201776+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: purchase_operations; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.purchase_operations (id, purchase_order_id, action, result_payload_json, processed_at_utc, tenant_id) FROM stdin;
\.


--
-- Data for Name: purchase_order_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.purchase_order_lines (id, purchase_order_id, product_id, quantity_ordered, quantity_received, quantity_returned, unit_price, tax_rate, line_net, line_tax, line_total, notes, tenant_id) FROM stdin;
6547803a-93f2-4e92-9fb5-2ad4ac8f7d73	97cbddf5-a747-4208-95aa-2a23573093d8	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	10000.000000	10000.000000	0.000000	0.5000	0.000000	5000.0000	0.0000	5000.0000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
0eb4bf86-0d21-4e57-a6ec-2d66d1dfc658	670f3186-3f68-45b0-a393-d4949a9273a7	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	10000.000000	10000.000000	0.000000	0.5000	0.000000	5000.0000	0.0000	5000.0000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
7f3f1c54-df60-4a8f-a966-fa5b83ec4697	6429c894-c6c9-4c80-8f12-bc6d9fb7ad3e	0126728d-23b2-45bf-8065-cfd7455d4db0	50.000000	50.000000	0.000000	5.0000	0.000000	250.0000	0.0000	250.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
ed76fdec-e34b-47ec-8788-99e87c300629	b4d3162e-3622-4da5-b8ae-dcf7f4f58598	0126728d-23b2-45bf-8065-cfd7455d4db0	30.000000	30.000000	0.000000	5.0000	0.000000	150.0000	0.0000	150.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
e0bdaba6-f398-48ba-8216-aaf429f2ac45	5b80e6ab-5072-45f5-a067-f53e02bcb37c	0126728d-23b2-45bf-8065-cfd7455d4db0	1.000000	1.000000	0.000000	5.0000	0.000000	5.0000	0.0000	5.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
a0c120f3-9043-4f0f-939e-3201499bdeec	f28d5866-d2cb-460d-b796-8495d3a743c3	0126728d-23b2-45bf-8065-cfd7455d4db0	1.000000	1.000000	0.000000	5.0000	0.000000	5.0000	0.0000	5.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
bef2137e-5ae5-4ed0-a46f-5bb72ec61992	f79c1098-ad0b-4a23-b68d-07ed8fdae1bc	0126728d-23b2-45bf-8065-cfd7455d4db0	10.000000	10.000000	0.000000	5.0000	0.000000	50.0000	0.0000	50.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
4d76f0e7-66ac-45b5-ba26-8992822cc8af	8a3830d8-d6f8-4f23-a01c-bba9431fa9d2	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	70.000000	70.000000	0.000000	150.0000	0.000000	10500.0000	0.0000	10500.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
1a50f3f0-5cf7-4dc4-aa41-a7a18f7a3325	f586da0b-269a-4407-9222-c2ce109ac009	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	50.000000	50.000000	0.000000	150.0000	0.000000	7500.0000	0.0000	7500.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: purchase_orders; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.purchase_orders (id, supplier_id, warehouse_id, location_id, status, payment_status, reference, notes, subtotal, tax_total, grand_total, paid_amount, created_by_user_id, created_at_utc, updated_at_utc, confirmed_at_utc, closed_at_utc, row_version, tenant_id) FROM stdin;
97cbddf5-a747-4208-95aa-2a23573093d8	9364dd79-a87b-436d-a35d-ab3585ef58ac	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	FullyReceived	Unpaid	1515	\N	5000.0000	0.0000	5000.0000	0.0000	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-27 23:57:15.145074+02	2026-09-27 23:57:25.802746+02	2026-09-27 23:57:20.301369+02	\N	4	e534cf35-c3f3-47e1-92a4-ed14befb3e91
670f3186-3f68-45b0-a393-d4949a9273a7	9364dd79-a87b-436d-a35d-ab3585ef58ac	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	FullyReceived	Unpaid	1212	\N	5000.0000	0.0000	5000.0000	0.0000	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:16:27.128358+02	2026-09-28 00:16:37.45147+02	2026-09-28 00:16:30.485759+02	\N	4	e534cf35-c3f3-47e1-92a4-ed14befb3e91
6429c894-c6c9-4c80-8f12-bc6d9fb7ad3e	9c13f046-f3e9-4a6d-85dc-5b0e40a31602	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	FullyReceived	Unpaid	121212	\N	250.0000	0.0000	250.0000	0.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:22:33.509593+02	2026-10-26 13:23:12.677646+02	2026-10-26 13:23:11.584851+02	\N	4	bd9cbe72-fe4c-4753-be04-d3040b554f67
b4d3162e-3622-4da5-b8ae-dcf7f4f58598	9c13f046-f3e9-4a6d-85dc-5b0e40a31602	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	FullyReceived	Unpaid	1515	\N	150.0000	0.0000	150.0000	0.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:29:55.432286+02	2026-10-26 13:29:59.354044+02	2026-10-26 13:29:58.424019+02	\N	4	bd9cbe72-fe4c-4753-be04-d3040b554f67
5b80e6ab-5072-45f5-a067-f53e02bcb37c	9c13f046-f3e9-4a6d-85dc-5b0e40a31602	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	FullyReceived	Unpaid	12	\N	5.0000	0.0000	5.0000	0.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 10:30:56.847798+02	2026-10-29 10:31:02.898424+02	2026-10-29 10:31:01.758063+02	\N	4	bd9cbe72-fe4c-4753-be04-d3040b554f67
f28d5866-d2cb-460d-b796-8495d3a743c3	9c13f046-f3e9-4a6d-85dc-5b0e40a31602	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	FullyReceived	Unpaid	12	\N	5.0000	0.0000	5.0000	0.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 10:31:20.828125+02	2026-10-29 10:31:25.502159+02	2026-10-29 10:31:23.717797+02	\N	4	bd9cbe72-fe4c-4753-be04-d3040b554f67
f586da0b-269a-4407-9222-c2ce109ac009	9c13f046-f3e9-4a6d-85dc-5b0e40a31602	3c48cd3b-f770-41d4-8889-94f96cc3eaef	2117a1cb-b030-48a6-b3ac-802116e23052	FullyReceived	Paid	\N	\N	7500.0000	0.0000	7500.0000	7500.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 11:35:50.756672+02	2026-10-29 12:59:03.812345+02	2026-10-29 11:35:53.041189+02	\N	5	bd9cbe72-fe4c-4753-be04-d3040b554f67
8a3830d8-d6f8-4f23-a01c-bba9431fa9d2	9c13f046-f3e9-4a6d-85dc-5b0e40a31602	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	FullyReceived	Paid	\N	\N	10500.0000	0.0000	10500.0000	10500.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 11:33:43.961774+02	2026-10-29 12:59:06.508645+02	2026-10-29 11:33:49.949613+02	\N	5	bd9cbe72-fe4c-4753-be04-d3040b554f67
f79c1098-ad0b-4a23-b68d-07ed8fdae1bc	9c13f046-f3e9-4a6d-85dc-5b0e40a31602	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	FullyReceived	Paid	12	\N	50.0000	0.0000	50.0000	50.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 10:31:57.698508+02	2026-10-29 12:59:08.434578+02	2026-10-29 10:32:00.544766+02	\N	5	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: purchase_receipt_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.purchase_receipt_lines (id, purchase_receipt_id, purchase_order_line_id, product_id, quantity, tenant_id) FROM stdin;
4f4f63f0-710c-4e9d-920b-fddcec5656a1	e0e5f410-12bc-42c0-93ac-366315fea946	6547803a-93f2-4e92-9fb5-2ad4ac8f7d73	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	10000.000000	e534cf35-c3f3-47e1-92a4-ed14befb3e91
2ddc2964-995f-4d7a-bda9-c4fda253cc65	48f6dad3-6d76-48b0-9b93-afac030634fb	0eb4bf86-0d21-4e57-a6ec-2d66d1dfc658	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	10000.000000	e534cf35-c3f3-47e1-92a4-ed14befb3e91
efa6bf2b-592b-4d12-b068-b34c6807ef7c	40588805-f807-4548-bb45-4d502b6e6521	7f3f1c54-df60-4a8f-a966-fa5b83ec4697	0126728d-23b2-45bf-8065-cfd7455d4db0	50.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
77b7c880-1457-479b-9226-39ce689dbc34	8d29e380-fe16-44f6-bcee-6c37ad836ca2	ed76fdec-e34b-47ec-8788-99e87c300629	0126728d-23b2-45bf-8065-cfd7455d4db0	30.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
143ee1aa-b983-4cf5-a2bb-9ccfcb805009	47c0d68a-60cc-4b99-936a-404c4d346696	e0bdaba6-f398-48ba-8216-aaf429f2ac45	0126728d-23b2-45bf-8065-cfd7455d4db0	1.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
d0ed5b12-f203-42fa-a06d-4bbcbc0724e9	8d5b7beb-39f1-4339-99fd-e14d14972c56	a0c120f3-9043-4f0f-939e-3201499bdeec	0126728d-23b2-45bf-8065-cfd7455d4db0	1.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
8b6cf364-ed32-4f15-9816-3ae80a465102	615556e5-61b9-412d-9a6f-22bc2927eda4	bef2137e-5ae5-4ed0-a46f-5bb72ec61992	0126728d-23b2-45bf-8065-cfd7455d4db0	10.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
1eedddf1-f081-4712-9b8c-14fc8e0ca5e3	c3dc5abb-99c0-4eab-9635-dd0f6bb039da	4d76f0e7-66ac-45b5-ba26-8992822cc8af	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	70.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
01de5091-eb65-486d-87b5-2131e3f9aba6	fbef4059-14be-48b1-8edd-3adaaa644816	1a50f3f0-5cf7-4dc4-aa41-a7a18f7a3325	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	50.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: purchase_receipts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.purchase_receipts (id, purchase_order_id, warehouse_id, location_id, notes, created_by_user_id, operation_id, received_at_utc, tenant_id) FROM stdin;
e0e5f410-12bc-42c0-93ac-366315fea946	97cbddf5-a747-4208-95aa-2a23573093d8	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	\N	2026-09-27 23:57:25.802746+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
48f6dad3-6d76-48b0-9b93-afac030634fb	670f3186-3f68-45b0-a393-d4949a9273a7	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	\N	2026-09-28 00:16:37.45147+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
40588805-f807-4548-bb45-4d502b6e6521	6429c894-c6c9-4c80-8f12-bc6d9fb7ad3e	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-26 13:23:12.677646+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
8d29e380-fe16-44f6-bcee-6c37ad836ca2	b4d3162e-3622-4da5-b8ae-dcf7f4f58598	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-26 13:29:59.354044+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
47c0d68a-60cc-4b99-936a-404c4d346696	5b80e6ab-5072-45f5-a067-f53e02bcb37c	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-29 10:31:02.898424+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
8d5b7beb-39f1-4339-99fd-e14d14972c56	f28d5866-d2cb-460d-b796-8495d3a743c3	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-29 10:31:25.502159+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
615556e5-61b9-412d-9a6f-22bc2927eda4	f79c1098-ad0b-4a23-b68d-07ed8fdae1bc	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-29 10:32:08.811043+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
c3dc5abb-99c0-4eab-9635-dd0f6bb039da	8a3830d8-d6f8-4f23-a01c-bba9431fa9d2	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-29 11:33:51.731263+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
fbef4059-14be-48b1-8edd-3adaaa644816	f586da0b-269a-4407-9222-c2ce109ac009	3c48cd3b-f770-41d4-8889-94f96cc3eaef	2117a1cb-b030-48a6-b3ac-802116e23052	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-29 11:36:05.563013+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: purchase_return_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.purchase_return_lines (id, purchase_return_id, purchase_order_line_id, product_id, quantity, tenant_id) FROM stdin;
\.


--
-- Data for Name: purchase_returns; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.purchase_returns (id, purchase_order_id, warehouse_id, location_id, notes, created_by_user_id, operation_id, returned_at_utc, tenant_id) FROM stdin;
\.


--
-- Data for Name: refresh_tokens; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.refresh_tokens (id, user_id, device_id, family_id, token_hash, created_at_utc, expires_at_utc, revoked_at_utc, replaced_by_token_id, tenant_id) FROM stdin;
25bf1736-dbe8-4953-8379-9007338d6be2	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	b53fafcd-24eb-4a42-9fef-2998fafc9858	a339626223a0777b3bc9a025313fae1ac76d9fbaa2940c2404d2cb32b9ad6efa	2026-09-27 23:12:47.960424+02	2026-10-11 23:12:47.960424+02	\N	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
d69fb25e-bc01-4716-bc02-94508986c437	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	bad4381c-4dfe-40af-9277-f9d1c975fe36	ba941dc5850b21b4e6432bdd175c53bf23cb65f397feb2d8ebce6f198962933a	2026-09-27 23:53:32.975747+02	2026-10-11 23:53:32.975747+02	\N	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
5c3e27f9-a229-484b-9cc5-cdfb4e52daeb	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	6543cb93-fd25-4d34-9f03-7ee5453f5cdf	f7ead0a84886459d46f147a27f5b21f800de5e0203150b1f8faed61d08a0998a	2026-09-28 00:14:30.710503+02	2026-10-12 00:14:30.710503+02	\N	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
8f8e5161-674f-4802-af89-2cebab133caa	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	e8ab56d0-b8eb-4ef4-a421-bde02a97d420	f5698dcc8c54aef091d905a58a3fcc101d2ef4630a63cd6f9529b3b58efa3a50	2026-09-29 11:02:41.654072+02	2026-10-13 11:02:41.654072+02	\N	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
937eb503-687d-4820-86f5-d8e54ac0d68a	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	c7a3e631-9936-4d58-8e5f-22e17bfd953f	756090d75ea16a400387ddcba740102e349cd9ddec9d1248b48d09b5ff77fa75	2026-09-29 12:35:23.270433+02	2026-10-13 12:35:23.270433+02	\N	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
937c3731-4b05-4166-913a-255ac03f10a4	813a8227-bc81-4580-ba4e-598cb2bf2231	7a37ba84-62ff-4d03-a15f-0c14ec5ae373	9c235570-df10-459f-8baf-e1192432e1e1	c0a5b655ce0257f54528f7ec5eaaf40564d708042c7d2dfb382100aeff3e14ce	2026-09-29 23:17:55.966384+02	2026-10-13 23:17:55.966384+02	\N	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
4ad99258-a208-423a-a47b-c5a7790c3c2d	b3b30173-470c-49f7-9991-8f4e0dbea3de	782ad8a3-5bb9-45f3-9f58-2da06c249a08	76340ed5-cfa7-44ec-8b98-61162f8f0c56	6c3928e7f250a7f1733735a49e09b7c7af6b74f7719d2d6de8163706957f6e82	2026-10-26 13:15:48.77465+02	2026-11-09 13:15:48.77465+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
78cb673b-d5f6-4dc7-a85f-e7a6c2ad953c	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	f43e9cab-4039-42b3-aa5f-a82979cd08e4	4c9e3e3a69e8bdbdd9badfa8f69f62c8065035c81913b3b3fe937e3391988b37	2026-10-26 13:17:23.922271+02	2026-11-09 13:17:23.922271+02	2026-10-26 13:23:43.509479+02	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
b34327fc-01e0-4fd0-b221-2b257b519785	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	73e24757-eec6-497c-a2c3-6bb27b9b4f81	97642a75b4852a0b11c45d0663959fa91dea68b6d17fcde603a8146b1230d495	2026-10-26 13:25:46.500339+02	2026-11-09 13:25:46.500339+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
20d9d32c-8823-4c44-889e-58d5efd5876b	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	7cda0e5f-5891-4c75-9399-b486c6bdb397	9516c6e489f131dd3feb9cde9b8fade5c98fa5519ed87905ffda87ca7069bb69	2026-10-26 13:26:24.850574+02	2026-11-09 13:26:24.850574+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
52876407-8c7f-47f8-bc1e-439102f6bbb7	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	726507ec-dfa7-4881-be9a-ce7ced0c010a	f54c60dcf5840a7d154840fb9b7dceab83f583d39208d7a746cffadee9106ebe	2026-10-26 13:24:23.952261+02	2026-11-09 13:24:23.952261+02	2026-10-26 13:26:54.264096+02	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
1977f337-5ba2-45fa-8536-89f5537a1280	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	3aa21565-5a6d-4005-b594-7dbf33957616	ec2632b6d8d80f9612853c80c7cc9f0472b582719f7d7ae361bb52266e34c773	2026-10-26 13:27:08.49961+02	2026-11-09 13:27:08.49961+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
ecb837b2-103a-4b43-8256-4977a9c40fd1	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	1c3a9641-1e06-4958-bf22-89de338552d0	9ce47633416553512dc4927af5ee68ce9af4edf6111ca7e3ab09d2b184a14df9	2026-10-26 21:40:21.916653+02	2026-11-09 21:40:21.916653+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
9a87445e-95f2-4d49-a138-fea24aa03561	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	a6fa454b-2783-46a9-be4f-a01b22f28556	cb5b389a774fefa1975824475e4ff2325d3707635de18607261662790e3569ad	2026-10-27 11:13:29.739238+02	2026-11-10 11:13:29.739238+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
3f1691a7-f809-4979-8454-446aa19d8ef5	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	5428f49f-b300-4799-b345-06bfcb3e1688	befd602e5dec551ae26ee863a92b4288b26deee5ca20abfc45679123d7f8474a	2026-10-29 10:29:01.536347+02	2026-11-12 10:29:01.536347+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
142182a7-b939-4fd7-bcb7-3f2270c87b5a	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	a8e0526c-d2c1-4277-b7b6-cf091295149e	7c3a0b517febb9b383459bd0b5ccb1429738985c726b85786d0912c7cc5f08cf	2026-10-29 10:58:21.398801+02	2026-11-12 10:58:21.398801+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
1d90db75-187f-4b3b-8624-73eec7e870b5	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	d2168c67-6c3b-4cac-8c51-2a395c8ac1f3	d557a6a8e5be8eae349a737ea456ae727241c86b86b037fc16b0dd693eab9d83	2026-10-29 11:15:18.964012+02	2026-11-12 11:15:18.964012+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
da0fa527-10fb-464c-9ec8-e4017d179076	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	332ce4e9-9e63-40df-bc6a-12a5974027fb	d63f229bb6d83c132c9c2739cf0690b910e39a50f3a04e508deea51269e61be3	2026-10-29 11:33:26.637772+02	2026-11-12 11:33:26.637772+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
a71501af-db3e-4577-afb6-2cea521349ec	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	d4aac2d4-e3bb-4ea3-ae51-9c59eebaf59a	c20313cce6b24cc780338358b4db1369e556fc98f7a97a880e78b25730cdeb95	2026-10-29 11:55:57.972602+02	2026-11-12 11:55:57.972602+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
9e0d72df-4900-4bff-a755-60a97a1aabd0	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	667f2ba9-f2b0-4bc3-a01b-c2b4cd8a1d4d	3c3f15a1da2bba48a8954bea1b4abd95110c099d8505b339d97ae74a69881766	2026-10-29 12:45:59.554535+02	2026-11-12 12:45:59.554535+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
5c4d3e73-477e-46c8-addf-8c294e7467fa	b3b30173-470c-49f7-9991-8f4e0dbea3de	3ccbcaf8-b061-4afd-9f79-c204669e8eb0	151bb86d-0049-4dbd-b363-9c767c884c15	bd88b17173a4eaed4f4a045250c33a0ed49fb348bfa2ce5f9205fc44eb3cb849	2026-10-30 12:13:45.638438+02	2026-11-13 12:13:45.638438+02	\N	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: restaurant_open_orders; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.restaurant_open_orders (id, warehouse_id, table_id, status, notes, opened_at_utc, closed_at_utc, pos_sale_id, tenant_id) FROM stdin;
\.


--
-- Data for Name: restaurant_tables; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.restaurant_tables (id, warehouse_id, code, name, is_active, tenant_id) FROM stdin;
\.


--
-- Data for Name: role_permissions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.role_permissions (id, role_id, permission_id, created_at_utc, tenant_id) FROM stdin;
005502ae-f4eb-45ed-994a-e2da7d291c28	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111105	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
015c45cc-ba47-4e58-94f7-613fa1dae937	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111110	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
01e564ce-350f-4fb6-9799-301b3a46fc48	9b77536d-be17-4463-8cff-a04ad187c9af	11111111-1111-1111-1111-111111111111	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
020c8b55-65ae-44e2-bc92-cf147140ebe3	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111103	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
047066dc-b93b-49f9-8a97-dcb35e286928	9b77536d-be17-4463-8cff-a04ad187c9af	11111111-1111-1111-1111-111111111105	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
066871fa-cdad-40b4-95f1-e294274a80eb	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111113	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
0f6d9790-ed32-489b-827f-b2475e61e176	12338c1b-d62f-4644-a967-cc3bb1f5aa2f	11111111-1111-1111-1111-111111111101	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
101fac01-611b-4ecb-80a7-266a4875c82a	12338c1b-d62f-4644-a967-cc3bb1f5aa2f	11111111-1111-1111-1111-111111111114	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
10d65952-72e1-40b4-879f-675f6d2e726f	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111105	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
146b0a6a-5eaa-48dc-9e42-2bc314917ee4	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111109	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
1b385433-d604-4e79-8529-9f6a5311ef4c	9b77536d-be17-4463-8cff-a04ad187c9af	11111111-1111-1111-1111-111111111106	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
1f970684-0ce0-4819-a7bf-78e70a12d493	e2408bc4-70d5-4d3a-b3b2-667493750442	11111111-1111-1111-1111-111111111107	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
270cdd14-5309-436d-b52e-c92de59c222a	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111101	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
27590801-3383-4e98-b6b2-7d15079f868c	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111115	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
2e53207c-d5a9-4fd8-a642-189e25251afb	f0547cc5-d554-4b5c-b39e-53d68a1733dd	11111111-1111-1111-1111-111111111112	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
317d7e0d-bf5c-483a-9b43-48c88d2dc197	09a86d8e-26fe-4a5d-8257-19f8a32922bf	11111111-1111-1111-1111-111111111114	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
3456340c-763d-4d22-baf4-d313e83a1869	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111110	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
35d4abeb-99b2-4232-a71c-ca24d814d95d	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111107	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
37769861-f57c-40e3-9b50-e408f36fb66b	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111108	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
3d84a015-bf70-4287-8777-2a0da2494def	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111114	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
3fb23bc2-599a-4327-8c02-8fd6fdfbfb72	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111107	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
4877b6a9-0234-49c4-b29d-091f088916f8	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111115	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
4b1dddaf-1bce-4579-8d12-316bd5832623	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111102	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
556e206c-4226-428d-980a-0826cf280b5e	e2408bc4-70d5-4d3a-b3b2-667493750442	11111111-1111-1111-1111-111111111109	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
558710be-fdc6-43b3-b692-29e43a012e96	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111109	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
684f504a-b8fe-4e0a-98ef-bfc6d9a843d2	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111105	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
6c0b78e7-f7a3-4d9b-908b-bdc5dc03fc99	09a86d8e-26fe-4a5d-8257-19f8a32922bf	11111111-1111-1111-1111-111111111105	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
715f9259-1451-4ee7-a85d-ad92aac849be	f0547cc5-d554-4b5c-b39e-53d68a1733dd	11111111-1111-1111-1111-111111111101	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
723793d5-c50a-4877-a62e-1d3e98bbb61e	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111106	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
79740492-891d-4179-99fa-50e4dd715594	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111103	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
857984ad-f261-48f9-ad39-d0486f79f03c	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111108	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
86179175-093e-4a37-af73-effdda8fbbbd	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111112	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
8a54aff8-d3e5-49b2-a4ca-1a4c2785bb51	9b77536d-be17-4463-8cff-a04ad187c9af	11111111-1111-1111-1111-111111111101	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
9120a51d-c99d-45e5-9759-5c6010a35bfb	09a86d8e-26fe-4a5d-8257-19f8a32922bf	11111111-1111-1111-1111-111111111101	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
91b2009e-e936-4781-9422-02001c5eb515	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111102	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
9a871b11-d216-4637-a348-624123520e17	12338c1b-d62f-4644-a967-cc3bb1f5aa2f	11111111-1111-1111-1111-111111111105	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
9aca3046-8bd3-4619-b831-f5d47f9bef2e	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111111	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
9d66a53f-eb74-4c1a-b25b-b4083342796d	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111108	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
a0d19035-19c1-497d-b1e6-f441e489a0c5	f0547cc5-d554-4b5c-b39e-53d68a1733dd	11111111-1111-1111-1111-111111111113	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
a1cea856-0dcd-4650-a828-316bbc8fa8b5	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111111	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
a546cee2-9aa3-461c-99df-a7d393c14eef	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111106	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
a8b35ef7-01cf-474f-875b-14837133ab00	e2408bc4-70d5-4d3a-b3b2-667493750442	11111111-1111-1111-1111-111111111105	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
ab992951-de27-4f38-86f0-e7d3d7689166	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111102	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
af1d3789-558d-40e4-a7a7-2d4e0a9d508c	e2408bc4-70d5-4d3a-b3b2-667493750442	11111111-1111-1111-1111-111111111101	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
bfeb31ac-e837-47d9-8e05-df6441c8cfa1	e2408bc4-70d5-4d3a-b3b2-667493750442	11111111-1111-1111-1111-111111111106	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
c3d950e5-e252-4eab-984e-f609dc4204d2	9b77536d-be17-4463-8cff-a04ad187c9af	11111111-1111-1111-1111-111111111110	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
c4dd2e62-8177-4563-ab81-5aed529b872c	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111104	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
cdce4545-9b62-478d-a3fb-8db0b29c5618	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111114	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
ce5781ae-494a-43ee-9520-f7a7a70bd435	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111113	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
d3e6b866-cef9-4e57-b996-52539eb1f230	f0547cc5-d554-4b5c-b39e-53d68a1733dd	11111111-1111-1111-1111-111111111105	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
d9e631e9-99fb-486e-87c4-c85d5021e9f3	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111106	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
e22be243-6a28-4494-bb17-fc7290b7d49e	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111103	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
e3854c01-8296-416e-8278-9599c2912262	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111104	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
e78ccb83-61d5-4066-ba38-aa41a910f954	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111101	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
e9734d39-cdaa-4a0e-b4d5-b750798f1267	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111101	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
eb2d7f33-4067-4e21-9fa0-c4f01eb26be3	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111112	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
ec3a81bf-d41d-4483-88e7-c93c0fe948bd	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111104	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
f2c15826-dad1-43bd-b681-1825f6d354c3	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111114	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
fc529788-0765-4fea-85dc-83a425551be4	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111107	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
fde1d5d3-6ae9-4f66-84a4-965bc47c15d8	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111109	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
0523f21c-5c55-4787-950b-deca9bb28165	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111109	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
0731cb93-b555-4716-96fa-86972d26ff99	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111104	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
14a2b405-49ba-492f-86ef-b229fa0ebfcb	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111102	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
1944200e-2385-4f4d-92d0-f29050a7ed46	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111103	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
22319692-1f3d-4516-ac7d-412ae4bc01df	c0c1fdb5-32ee-4639-89da-eeade24fe0d0	11111111-1111-1111-1111-111111111101	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
2d9b207d-0ec8-4c7a-bee1-974033a7893b	6d8ec3bc-2559-4b1c-9e6f-dc1bba90c0e5	11111111-1111-1111-1111-111111111114	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
332728ff-b42e-4f72-991c-e3e0403950c4	c0c1fdb5-32ee-4639-89da-eeade24fe0d0	11111111-1111-1111-1111-111111111105	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
37d22047-f22a-4198-9f59-bcb968dc25a2	8c63dc2c-26d6-4b30-b276-768328b42481	11111111-1111-1111-1111-111111111105	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
38a66c87-3e3c-4c96-b9ee-2622c39cae93	1990dbff-be27-43fa-a076-253ee5ddcbd7	11111111-1111-1111-1111-111111111109	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
39d9a8a2-2c6a-4bd4-b241-a08a3c573ac6	f2f81249-1716-4a27-a0c4-c4db514969ee	11111111-1111-1111-1111-111111111101	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
41e46c4a-3d7a-4285-98ad-2d70e4558a86	f2f81249-1716-4a27-a0c4-c4db514969ee	11111111-1111-1111-1111-111111111105	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
4bf0e783-efcc-43db-905c-5298921f4052	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111101	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
4c138a9c-87fc-4a20-ad7d-31c9afeff770	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111105	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
56482c94-2d49-4426-9abc-09da71d66da2	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111110	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
56a46ec4-cdc6-448c-a5a7-8f3657888398	6d8ec3bc-2559-4b1c-9e6f-dc1bba90c0e5	11111111-1111-1111-1111-111111111101	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
58a630b6-0426-4314-80a4-b3738314a38a	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111112	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
594f4b21-8454-4203-a6f7-f3a5d20fe32e	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111102	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
5d9f9158-a9ef-409a-ae32-74a388faaee0	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111106	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
61d634c4-4e48-4736-8c4d-03d335c79689	1990dbff-be27-43fa-a076-253ee5ddcbd7	11111111-1111-1111-1111-111111111106	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
62217edb-1048-40a7-8510-0b1df26bd7f9	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111108	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
62838ba0-1c5b-41c3-909e-674de367c221	1990dbff-be27-43fa-a076-253ee5ddcbd7	11111111-1111-1111-1111-111111111107	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
62f76397-301f-4c13-b095-f8e0360684d6	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111109	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
633c192e-f126-443f-ad93-27d429b44ff4	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111112	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
69f16d68-3131-4e12-8d6a-757e51a61f9e	1990dbff-be27-43fa-a076-253ee5ddcbd7	11111111-1111-1111-1111-111111111105	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
6b705aad-064f-4fb5-a41d-f71f3bb35729	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111115	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
73d203e7-7df1-4de7-989b-bbaf8b8a677f	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111107	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
75593a5f-a271-4c1f-8e72-601a6cf97ec5	1990dbff-be27-43fa-a076-253ee5ddcbd7	11111111-1111-1111-1111-111111111101	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
76b08630-cc15-4278-817c-5f2e818b5319	8c63dc2c-26d6-4b30-b276-768328b42481	11111111-1111-1111-1111-111111111101	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
78540b17-be7c-4ef5-9ccb-852a7ee4d820	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111106	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
7a4830fa-4903-48ad-a6cd-6784cbd10c50	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111114	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
7d1aa53b-a84d-46c2-9829-07064382dd6c	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111108	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
7ddeca11-ba66-404e-a0e4-f7f819ca1260	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111111	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
8487ed8d-a161-4cd0-b927-c867c811b366	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111107	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
8c5aa43d-c763-420c-9080-51b71413df54	f2f81249-1716-4a27-a0c4-c4db514969ee	11111111-1111-1111-1111-111111111114	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
8edac2f1-0e78-438c-8094-3b91c1033c08	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111113	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
913c99b0-ea2f-4887-8953-f1b541b6647a	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111109	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
93a79b05-2b18-42a1-8162-9b1720b90c53	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111103	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
a534d3fd-77e5-47e7-a130-b2de08170da7	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111115	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
ad7ba48b-95d1-4f82-b9b8-8986a30541f0	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111111	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
ae0fb56a-9dac-4d0d-b2b6-24d79d030874	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111102	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
b40453cd-7cd6-4109-bf45-11c630bf5c0b	c0c1fdb5-32ee-4639-89da-eeade24fe0d0	11111111-1111-1111-1111-111111111111	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
b58f8ab5-f868-455b-9999-38a80acd33eb	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111113	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
bef45879-7ec8-4119-ba03-98f95990fb26	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111105	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
c18d4277-0fbf-46c4-9c78-0a6af3c48348	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111114	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
d34fb2d0-70cb-4a1c-ba07-2e9920ecb544	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111105	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
d6008ff6-6f89-4632-bad6-2eb537ab3e21	6d8ec3bc-2559-4b1c-9e6f-dc1bba90c0e5	11111111-1111-1111-1111-111111111105	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
dbecf36c-6dc6-480b-a04a-aee027c233c4	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111101	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
de93273e-6a15-455b-b787-f7093b50c1ad	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111101	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
df9942a4-6f83-4789-8ad2-25a52570ca3f	c0c1fdb5-32ee-4639-89da-eeade24fe0d0	11111111-1111-1111-1111-111111111106	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
e71fd077-4ad9-4ff2-b4e0-e8a9ab22b885	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111104	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
e81ff5fa-492c-4ef8-b6fd-4fe5f870e280	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111110	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
ea866c51-51b5-4ef9-94a1-77319be6467b	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111106	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
ebacf9e3-2a3a-4620-b71a-45042325ecc4	8c63dc2c-26d6-4b30-b276-768328b42481	11111111-1111-1111-1111-111111111112	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
ec9ea100-8fed-46e9-8f70-416fb26b1515	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111114	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
ede80095-8693-4ecf-a850-0407a9c8b432	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111107	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
efe3f1da-9929-4c9a-986d-2e9770e77ccb	c0c1fdb5-32ee-4639-89da-eeade24fe0d0	11111111-1111-1111-1111-111111111110	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
f25b80d6-298f-441f-a9f9-53ee0060432b	8c63dc2c-26d6-4b30-b276-768328b42481	11111111-1111-1111-1111-111111111113	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
f6117eb9-7b3c-46bd-9ee6-48bc0224918d	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111108	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
f65b4bad-b9f0-4c7f-bfdd-6320652ca763	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111104	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
fe371c93-fe9e-466d-8f65-ad9d21a44736	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111103	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
3da77c8a-9233-4e43-bbba-418b1fa8bfd1	f0547cc5-d554-4b5c-b39e-53d68a1733dd	11111111-1111-1111-1111-111111111117	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
a69009de-d210-412e-a46a-9028faa067d7	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111117	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
b96f50cd-2555-4da3-a12f-3e074b757d1b	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111119	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
5163a1ed-9147-49f0-bfbf-6121c3204b72	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111118	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
b7dba6b3-2a0a-448b-8b11-f35c8760dd91	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111116	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
80bdd323-11df-4be7-ad73-0ecfe8fc896f	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111117	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
054cc976-82d7-42b3-ba91-53d9f43dfe62	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111119	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
2f573ec4-f910-42de-9bc2-166191961ed9	f0547cc5-d554-4b5c-b39e-53d68a1733dd	11111111-1111-1111-1111-111111111119	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
d7d68e50-8c6c-4f5f-9a23-fdbf149c0358	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111119	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
dabf238c-ef91-43dc-b105-b39133ddb44c	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111117	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
f491717c-956b-45e0-9c99-8abe3512feab	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111119	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
2f3d2cc6-daf7-4a7a-9315-07c70811ef2a	8c63dc2c-26d6-4b30-b276-768328b42481	11111111-1111-1111-1111-111111111116	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
1827bd4f-a174-4d2c-a87d-dd5f00948e5a	8c63dc2c-26d6-4b30-b276-768328b42481	11111111-1111-1111-1111-111111111118	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
7ea4f761-d2c2-45ed-924e-9240e90c848b	f0547cc5-d554-4b5c-b39e-53d68a1733dd	11111111-1111-1111-1111-111111111107	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
23830b6b-3adc-471c-8dcb-8db5ca2fa6e7	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111117	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
3e5b0815-1f92-4a47-9a24-ba338e48de7c	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111119	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
c0d7981c-73ee-4bb1-bf3a-c475b905a757	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111117	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
d1df6a6a-aa2e-4b1e-90d1-d6a6c7ff5d93	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111116	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
df8a3761-4751-4835-8613-5797794ab0f4	ec7dcfac-e6f6-45d1-9f21-29995afe4d93	11111111-1111-1111-1111-111111111118	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
7e04374d-a502-46bb-b718-2758afd3b78c	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111119	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
c825d08c-401b-4a6b-8bcc-fb849b7b56b4	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	11111111-1111-1111-1111-111111111117	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
9d4be7f3-f500-4b3c-8a91-9dcff9a8052a	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111116	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
fc58ad10-d370-4379-885b-ea906823816d	16011bf5-2921-471d-b401-21a4c02fbb06	11111111-1111-1111-1111-111111111118	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
178a091f-5564-4a92-b822-c7294f508f89	8c63dc2c-26d6-4b30-b276-768328b42481	11111111-1111-1111-1111-111111111107	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
de7f18e0-c569-4b1a-9bf6-b417c986404e	f0547cc5-d554-4b5c-b39e-53d68a1733dd	11111111-1111-1111-1111-111111111118	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
5be8aef2-dc67-425c-a67f-f5800eadb2c9	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111118	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
2b467f24-fa48-4c5b-8cd4-bc50b4b28786	0580eb73-8039-4acf-b62c-4e97d7021594	11111111-1111-1111-1111-111111111116	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
80bf4332-a686-436b-a115-413b77eeeaab	f0547cc5-d554-4b5c-b39e-53d68a1733dd	11111111-1111-1111-1111-111111111116	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
7dc9c94b-4aa8-4b1a-9b1e-baf00ec20ac4	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111118	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
d56b21c2-fedc-4d36-ab0c-a28374eea205	8c63dc2c-26d6-4b30-b276-768328b42481	11111111-1111-1111-1111-111111111119	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
aa57a1d1-a563-4de6-b263-8482f7eef63c	e86d121b-5743-45b5-ad67-e93e6dc92c5f	11111111-1111-1111-1111-111111111116	2026-10-26 13:26:14.21238+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
b577d4ee-0a06-47fa-b8c2-9b7302ea5afc	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111116	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
4c26e73e-498b-4af4-9772-5b0f4252b9b2	74576701-aa61-47bf-b409-541620afc7ad	11111111-1111-1111-1111-111111111118	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
0bc94ae5-2d10-44b4-8ceb-ebb936f05fb1	8c63dc2c-26d6-4b30-b276-768328b42481	11111111-1111-1111-1111-111111111117	2026-10-26 13:26:14.21238+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
0f913b7a-d48e-4843-9753-6202ba3712d2	c6cdbdb0-5aa2-45cd-b8d3-b73d66225336	11111111-1111-1111-1111-111111111107	2026-10-29 10:57:32.571455+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
12d0519d-d1dd-4889-a08a-6aa6638bf4f2	c6cdbdb0-5aa2-45cd-b8d3-b73d66225336	11111111-1111-1111-1111-111111111105	2026-10-29 10:57:32.571455+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
196b5ddc-c547-4b19-a9b2-11e84e3b7c66	c6cdbdb0-5aa2-45cd-b8d3-b73d66225336	11111111-1111-1111-1111-111111111116	2026-10-29 10:57:32.571455+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
327cbffd-2f52-41ed-a1c5-873cebdc2d31	04f6bf7f-87b3-4e61-be71-3339400faf01	11111111-1111-1111-1111-111111111101	2026-10-29 10:57:32.571455+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
4a2f2645-14ae-4270-835b-4abd24fd19a4	04f6bf7f-87b3-4e61-be71-3339400faf01	11111111-1111-1111-1111-111111111116	2026-10-29 10:57:32.571455+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
727655ac-75f6-4378-b248-a808deb78dbe	04f6bf7f-87b3-4e61-be71-3339400faf01	11111111-1111-1111-1111-111111111107	2026-10-29 10:57:32.571455+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
94a38f7f-e5f2-46de-9a2e-810fb88caa76	04f6bf7f-87b3-4e61-be71-3339400faf01	11111111-1111-1111-1111-111111111105	2026-10-29 10:57:32.571455+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
bef4b5bd-9aa5-4b61-b2c8-ef6bb2a43a21	04f6bf7f-87b3-4e61-be71-3339400faf01	11111111-1111-1111-1111-111111111119	2026-10-29 10:57:32.571455+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
d924d024-a4bc-4e99-8d67-4e2e1d47f46c	c6cdbdb0-5aa2-45cd-b8d3-b73d66225336	11111111-1111-1111-1111-111111111101	2026-10-29 10:57:32.571455+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
e2fd45e9-61af-424f-9bc3-6656560d638e	c6cdbdb0-5aa2-45cd-b8d3-b73d66225336	11111111-1111-1111-1111-111111111119	2026-10-29 10:57:32.571455+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.roles (id, code, name, is_system, is_active, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
09a86d8e-26fe-4a5d-8257-19f8a32922bf	Accountant	Accountant	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
12338c1b-d62f-4644-a967-cc3bb1f5aa2f	Viewer	Viewer	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
16011bf5-2921-471d-b401-21a4c02fbb06	WarehouseManager	Warehouse manager	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
9b77536d-be17-4463-8cff-a04ad187c9af	PurchasingUser	Purchasing user	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	Owner	Owner	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
e2408bc4-70d5-4d3a-b3b2-667493750442	WarehouseWorker	Warehouse worker	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
e86d121b-5743-45b5-ad67-e93e6dc92c5f	Administrator	Administrator	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
f0547cc5-d554-4b5c-b39e-53d68a1733dd	SalesUser	Sales user	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
0580eb73-8039-4acf-b62c-4e97d7021594	Owner	Owner	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
1990dbff-be27-43fa-a076-253ee5ddcbd7	WarehouseWorker	Warehouse worker	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
6d8ec3bc-2559-4b1c-9e6f-dc1bba90c0e5	Accountant	Accountant	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
74576701-aa61-47bf-b409-541620afc7ad	Administrator	Administrator	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
8c63dc2c-26d6-4b30-b276-768328b42481	SalesUser	Sales user	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
c0c1fdb5-32ee-4639-89da-eeade24fe0d0	PurchasingUser	Purchasing user	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
ec7dcfac-e6f6-45d1-9f21-29995afe4d93	WarehouseManager	Warehouse manager	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
f2f81249-1716-4a27-a0c4-c4db514969ee	Viewer	Viewer	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
04f6bf7f-87b3-4e61-be71-3339400faf01	Cashier	Cashier	t	t	2026-10-29 10:57:32.571455+02	2026-10-29 10:57:32.571455+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
c6cdbdb0-5aa2-45cd-b8d3-b73d66225336	Cashier	Cashier	t	t	2026-10-29 10:57:32.571455+02	2026-10-29 10:57:32.571455+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: sales_operations; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_operations (id, sales_order_id, action, result_payload_json, processed_at_utc, tenant_id) FROM stdin;
\.


--
-- Data for Name: sales_order_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_order_lines (id, sales_order_id, product_id, quantity_ordered, quantity_shipped, quantity_returned, unit_price, tax_rate, line_net, line_tax, line_total, notes, tenant_id) FROM stdin;
07b6a5ba-44c8-49e5-8fb1-0527b402bc63	8b3e033f-47fa-4295-b73d-c3bf38179157	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	1000.000000	1000.000000	0.000000	0.7500	0.000000	750.0000	0.0000	750.0000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
7ae8f32e-52a3-4e27-b7a1-ba85823f984f	542b14fa-1db7-43b4-a095-f49810cae00a	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2500.000000	2500.000000	0.000000	0.7500	0.000000	1875.0000	0.0000	1875.0000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
705f5420-466d-451d-a0a0-1ba9496c817b	0e1dd937-ea39-4fb0-b42d-4b3d697aa0ce	0126728d-23b2-45bf-8065-cfd7455d4db0	15.000000	15.000000	0.000000	6.0000	0.000000	90.0000	0.0000	90.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
9420dc1e-22f5-491e-8d64-85adfc1c1964	31607fcc-95c1-4ff0-a833-f86787a1a525	0126728d-23b2-45bf-8065-cfd7455d4db0	50.000000	50.000000	0.000000	6.0000	0.000000	300.0000	0.0000	300.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
dba5b676-6d8d-4009-aa07-306a425cf42b	cc367be7-6333-421e-b3a6-05b71290205a	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	55.000000	55.000000	0.000000	160.0000	0.000000	8800.0000	0.0000	8800.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
2f6a93dd-a462-4102-87cc-7b72fd9c6044	6e839bb3-e67d-484b-ad9f-1c3b03e26962	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	1.000000	1.000000	0.000000	160.0000	0.000000	160.0000	0.0000	160.0000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: sales_orders; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_orders (id, customer_id, warehouse_id, location_id, status, payment_status, reference, notes, subtotal, tax_total, grand_total, paid_amount, created_by_user_id, created_at_utc, updated_at_utc, confirmed_at_utc, closed_at_utc, row_version, tenant_id) FROM stdin;
8b3e033f-47fa-4295-b73d-c3bf38179157	8e6f8326-582f-44cf-b799-500e3e95722b	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	Fulfilled	Unpaid	\N	\N	750.0000	0.0000	750.0000	0.0000	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-27 23:58:06.203823+02	2026-09-27 23:58:10.056241+02	2026-09-27 23:58:08.761528+02	\N	4	e534cf35-c3f3-47e1-92a4-ed14befb3e91
542b14fa-1db7-43b4-a095-f49810cae00a	8e6f8326-582f-44cf-b799-500e3e95722b	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	Fulfilled	Unpaid	\N	\N	1875.0000	0.0000	1875.0000	0.0000	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:17:21.792094+02	2026-09-28 00:17:28.15863+02	2026-09-28 00:17:24.831772+02	\N	4	e534cf35-c3f3-47e1-92a4-ed14befb3e91
0e1dd937-ea39-4fb0-b42d-4b3d697aa0ce	2be50852-dade-4c13-b6c7-f4c08a148594	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	Fulfilled	Unpaid	\N	\N	90.0000	0.0000	90.0000	0.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:22:59.82986+02	2026-10-26 13:23:21.355734+02	2026-10-26 13:23:04.061204+02	\N	4	bd9cbe72-fe4c-4753-be04-d3040b554f67
31607fcc-95c1-4ff0-a833-f86787a1a525	2be50852-dade-4c13-b6c7-f4c08a148594	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	Fulfilled	Unpaid	\N	\N	300.0000	0.0000	300.0000	0.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 10:32:38.428257+02	2026-10-29 10:32:42.897923+02	2026-10-29 10:32:40.057727+02	\N	4	bd9cbe72-fe4c-4753-be04-d3040b554f67
cc367be7-6333-421e-b3a6-05b71290205a	2be50852-dade-4c13-b6c7-f4c08a148594	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	Fulfilled	Unpaid	\N	\N	8800.0000	0.0000	8800.0000	0.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 11:34:17.395929+02	2026-10-29 11:34:22.917307+02	2026-10-29 11:34:21.205158+02	\N	4	bd9cbe72-fe4c-4753-be04-d3040b554f67
6e839bb3-e67d-484b-ad9f-1c3b03e26962	2be50852-dade-4c13-b6c7-f4c08a148594	3c48cd3b-f770-41d4-8889-94f96cc3eaef	2117a1cb-b030-48a6-b3ac-802116e23052	Fulfilled	Paid	\N	\N	160.0000	0.0000	160.0000	160.0000	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 11:34:58.803165+02	2026-10-29 12:46:13.18359+02	2026-10-29 11:35:08.620094+02	\N	5	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: sales_return_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_return_lines (id, sales_return_id, sales_order_line_id, product_id, quantity, tenant_id) FROM stdin;
\.


--
-- Data for Name: sales_returns; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_returns (id, sales_order_id, warehouse_id, location_id, notes, created_by_user_id, operation_id, returned_at_utc, tenant_id) FROM stdin;
\.


--
-- Data for Name: sales_shipment_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_shipment_lines (id, sales_shipment_id, sales_order_line_id, product_id, quantity, tenant_id) FROM stdin;
58fc1c0b-63cf-454c-964a-2f5856041524	4067bd4e-b9bf-4bc8-b54a-22ef99e65bc0	07b6a5ba-44c8-49e5-8fb1-0527b402bc63	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	1000.000000	e534cf35-c3f3-47e1-92a4-ed14befb3e91
998c71df-cf7c-4ce2-ab7c-e2fa4b8aeeb3	3fbfc155-b5e8-4013-a149-6a186f6a10c7	7ae8f32e-52a3-4e27-b7a1-ba85823f984f	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2500.000000	e534cf35-c3f3-47e1-92a4-ed14befb3e91
92678b98-ddd9-4acd-ad86-178ff61de30f	7619bc07-516c-48a2-9109-918d8f6b5f77	705f5420-466d-451d-a0a0-1ba9496c817b	0126728d-23b2-45bf-8065-cfd7455d4db0	15.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
dc99f77e-b8dc-47f4-b7ff-15c931296028	536fcb67-133c-4b7e-bfca-4aa3eac3b7cc	9420dc1e-22f5-491e-8d64-85adfc1c1964	0126728d-23b2-45bf-8065-cfd7455d4db0	50.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
82623e41-4df0-4428-b402-0a9b3274f942	7b92f2a7-bd1f-4e80-ae27-73410847037f	dba5b676-6d8d-4009-aa07-306a425cf42b	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	55.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
d9046370-a9d8-4084-bfc2-ca7f43848dad	19a2b057-8df6-4e55-a5e3-5149200471b2	2f6a93dd-a462-4102-87cc-7b72fd9c6044	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	1.000000	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: sales_shipments; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_shipments (id, sales_order_id, warehouse_id, location_id, notes, created_by_user_id, operation_id, shipped_at_utc, tenant_id) FROM stdin;
4067bd4e-b9bf-4bc8-b54a-22ef99e65bc0	8b3e033f-47fa-4295-b73d-c3bf38179157	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	\N	2026-09-27 23:58:10.056241+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
3fbfc155-b5e8-4013-a149-6a186f6a10c7	542b14fa-1db7-43b4-a095-f49810cae00a	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	\N	2026-09-28 00:17:28.15863+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
7619bc07-516c-48a2-9109-918d8f6b5f77	0e1dd937-ea39-4fb0-b42d-4b3d697aa0ce	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-26 13:23:21.355734+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
536fcb67-133c-4b7e-bfca-4aa3eac3b7cc	31607fcc-95c1-4ff0-a833-f86787a1a525	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-29 10:32:42.897923+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
7b92f2a7-bd1f-4e80-ae27-73410847037f	cc367be7-6333-421e-b3a6-05b71290205a	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-29 11:34:22.917307+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
19a2b057-8df6-4e55-a5e3-5149200471b2	6e839bb3-e67d-484b-ad9f-1c3b03e26962	3c48cd3b-f770-41d4-8889-94f96cc3eaef	2117a1cb-b030-48a6-b3ac-802116e23052	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	\N	2026-10-29 11:36:21.908055+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: stock_adjustment_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.stock_adjustment_lines (id, stock_adjustment_id, product_id, location_id, quantity_change, notes, tenant_id) FROM stdin;
c40ee050-529c-4a79-87b1-45984b56c9f7	13d57f59-a2f0-4d62-a96d-b82292a090b3	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2b16062e-7425-4704-bc4b-d339c60bd3d3	10000.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
36f8c72d-41a1-470b-8c7e-1965c6c115d2	0ac2d693-cf50-44cf-a901-0d032cee981b	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	bae87cce-f597-4baa-8f26-158eecbdbeb2	15000.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
09521d4b-7198-4fd0-a594-2a2ddee299b4	33ab19c8-569d-4321-827b-02f4472d086c	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
aba81628-b072-4619-9afc-9d38bc3e4739	0ee056d2-9e27-4820-be4c-b317db941c0f	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
f241d3f4-388f-4d7a-accc-5bf87ea48553	64314585-d359-4406-bcf4-268a22994165	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
21804cba-871c-4492-83bd-6aac3069d761	c86b6338-333c-4ecb-91cd-66541a71ba24	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
dd630a99-8aea-4437-adb4-3e5413ceefcd	c05b6ffb-92cb-4639-943b-03a8de0a0ecf	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	bae87cce-f597-4baa-8f26-158eecbdbeb2	10.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
dfcbc7a4-2606-46c1-8b48-ab1fc6ead354	8ea6aaf6-5645-42f6-9a58-aaf8e4660dbb	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	bae87cce-f597-4baa-8f26-158eecbdbeb2	10.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
1270740d-4810-4cff-a0be-0e11e78c2b50	9450966c-d263-4d36-9fd5-336685af09bd	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
b7346b39-a0da-41e3-9eac-291f5eb83d11	13138ee8-1234-4fd3-88d6-69fb7096bca2	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
b5acce95-1569-44e2-bdc3-126061e45e16	2e492b52-4850-496f-8096-0099b4c5117c	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
4199f4a0-0807-4e79-a0e7-387e68cc8387	601b2323-a544-4e97-8592-28068abea11b	0126728d-23b2-45bf-8065-cfd7455d4db0	004058ee-f0d7-417c-8817-ff95f6371754	10.000000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
988ddc47-a412-4ed9-a0f5-b7a078c9c327	773f8dfb-4df8-4854-8a1d-22f90cbc082e	0126728d-23b2-45bf-8065-cfd7455d4db0	004058ee-f0d7-417c-8817-ff95f6371754	20.000000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: stock_adjustments; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.stock_adjustments (id, warehouse_id, kind, status, reason, created_by_user_id, created_at_utc, updated_at_utc, posted_at_utc, row_version, tenant_id) FROM stdin;
13d57f59-a2f0-4d62-a96d-b82292a090b3	9403ba49-33c1-443f-963b-b0cc0b321d6b	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-27 23:56:14.958233+02	2026-09-27 23:56:15.478667+02	2026-09-27 23:56:15.478667+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
0ac2d693-cf50-44cf-a901-0d032cee981b	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:04:30.757315+02	2026-09-28 00:04:30.807351+02	2026-09-28 00:04:30.807351+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
33ab19c8-569d-4321-827b-02f4472d086c	9403ba49-33c1-443f-963b-b0cc0b321d6b	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:08:03.384311+02	2026-09-28 00:08:03.418234+02	2026-09-28 00:08:03.418234+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
0ee056d2-9e27-4820-be4c-b317db941c0f	9403ba49-33c1-443f-963b-b0cc0b321d6b	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:08:07.16307+02	2026-09-28 00:08:07.209202+02	2026-09-28 00:08:07.209202+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
64314585-d359-4406-bcf4-268a22994165	9403ba49-33c1-443f-963b-b0cc0b321d6b	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:08:08.715289+02	2026-09-28 00:08:08.786134+02	2026-09-28 00:08:08.786134+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
c86b6338-333c-4ecb-91cd-66541a71ba24	9403ba49-33c1-443f-963b-b0cc0b321d6b	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:08:11.093252+02	2026-09-28 00:08:11.142088+02	2026-09-28 00:08:11.142088+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
c05b6ffb-92cb-4639-943b-03a8de0a0ecf	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:08:15.432557+02	2026-09-28 00:08:15.471654+02	2026-09-28 00:08:15.471654+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
8ea6aaf6-5645-42f6-9a58-aaf8e4660dbb	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:08:17.962623+02	2026-09-28 00:08:18.003645+02	2026-09-28 00:08:18.003645+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
9450966c-d263-4d36-9fd5-336685af09bd	9403ba49-33c1-443f-963b-b0cc0b321d6b	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:15:33.649761+02	2026-09-28 00:15:33.691731+02	2026-09-28 00:15:33.691731+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
13138ee8-1234-4fd3-88d6-69fb7096bca2	9403ba49-33c1-443f-963b-b0cc0b321d6b	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:15:37.240945+02	2026-09-28 00:15:37.278177+02	2026-09-28 00:15:37.278177+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
2e492b52-4850-496f-8096-0099b4c5117c	9403ba49-33c1-443f-963b-b0cc0b321d6b	OpeningBalance	Posted	Opening balance	813a8227-bc81-4580-ba4e-598cb2bf2231	2026-09-28 00:15:39.730513+02	2026-09-28 00:15:39.774272+02	2026-09-28 00:15:39.774272+02	2	e534cf35-c3f3-47e1-92a4-ed14befb3e91
601b2323-a544-4e97-8592-28068abea11b	bd357264-c8d1-472b-987d-575ac34ff384	OpeningBalance	Posted	Opening balance	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-26 13:21:10.412855+02	2026-10-26 13:21:10.926479+02	2026-10-26 13:21:10.926479+02	2	bd9cbe72-fe4c-4753-be04-d3040b554f67
773f8dfb-4df8-4854-8a1d-22f90cbc082e	bd357264-c8d1-472b-987d-575ac34ff384	OpeningBalance	Posted	Opening balance	b3b30173-470c-49f7-9991-8f4e0dbea3de	2026-10-29 10:30:08.383202+02	2026-10-29 10:30:08.454649+02	2026-10-29 10:30:08.454649+02	2	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: stock_balances; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.stock_balances (id, product_id, warehouse_id, location_id, on_hand, reserved, updated_at_utc, row_version, tenant_id) FROM stdin;
1726f15f-4173-42a2-a596-11299f220973	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	19069.000000	0.000000	2026-09-29 11:06:35.741296+02	12	e534cf35-c3f3-47e1-92a4-ed14befb3e91
3c2e02e1-ac53-4675-bd44-7819b757c81d	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	06696022-fc7e-4b0c-910d-f80ebee212cc	0.000000	0.000000	2026-09-29 11:06:38.548023+02	3	e534cf35-c3f3-47e1-92a4-ed14befb3e91
8fc2436c-2102-4287-b0dc-66bdc411259e	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	22521.000000	0.000000	2026-09-29 11:06:38.548023+02	7	e534cf35-c3f3-47e1-92a4-ed14befb3e91
b8d6c69a-e1ec-4474-a513-4ca7feff6b28	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	45.000000	0.000000	2026-10-29 10:32:42.897923+02	13	bd9cbe72-fe4c-4753-be04-d3040b554f67
bb676f19-5bf5-448c-a290-459f33f35d5f	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	3c48cd3b-f770-41d4-8889-94f96cc3eaef	2117a1cb-b030-48a6-b3ac-802116e23052	49.000000	0.000000	2026-10-29 11:36:21.908055+02	3	bd9cbe72-fe4c-4753-be04-d3040b554f67
d3143d7b-741f-488d-84ca-ea501d576dd2	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	13.000000	0.000000	2026-10-29 12:55:44.700188+02	4	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: stock_count_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.stock_count_lines (id, stock_count_id, product_id, system_quantity, counted_quantity, notes, tenant_id) FROM stdin;
73d05393-b788-4eb3-af1c-835d0bda60b4	b2b49dfc-f367-4f52-a607-bf50566a0bcb	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	19070.000000	19070.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
6ae5820c-3fac-4dbe-96a7-63477b644038	b7a813ac-de7b-4ccf-bf1f-05bf3b040f46	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	22520.000000	22520.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
1a53d751-6e16-4f13-ba68-a4ea93b13f9c	213d11df-c83d-49d4-98a4-26be379c4415	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	19070.000000	19070.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
ebd0b457-2475-4745-bdd4-16c278bad39a	955d557f-0e67-492c-87e1-0ac2bd5bbe63	0126728d-23b2-45bf-8065-cfd7455d4db0	63.000000	63.000000	\N	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: stock_counts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.stock_counts (id, warehouse_id, location_id, status, notes, created_at_utc, updated_at_utc, posted_at_utc, posted_adjustment_id, row_version, tenant_id) FROM stdin;
b2b49dfc-f367-4f52-a607-bf50566a0bcb	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	Posted	\N	2026-09-29 11:03:13.175646+02	2026-09-29 11:03:17.540865+02	2026-09-29 11:03:17.540865+02	\N	3	e534cf35-c3f3-47e1-92a4-ed14befb3e91
b7a813ac-de7b-4ccf-bf1f-05bf3b040f46	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	Posted	\N	2026-09-29 11:03:22.997109+02	2026-09-29 11:03:25.4921+02	2026-09-29 11:03:25.4921+02	\N	3	e534cf35-c3f3-47e1-92a4-ed14befb3e91
213d11df-c83d-49d4-98a4-26be379c4415	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	Posted	\N	2026-09-29 11:06:15.986893+02	2026-09-29 11:06:19.910717+02	2026-09-29 11:06:19.910717+02	\N	3	e534cf35-c3f3-47e1-92a4-ed14befb3e91
955d557f-0e67-492c-87e1-0ac2bd5bbe63	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	Posted	\N	2026-10-27 11:13:48.430505+02	2026-10-27 11:13:57.390497+02	2026-10-27 11:13:57.390497+02	\N	3	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: stock_movements; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.stock_movements (id, product_id, warehouse_id, location_id, quantity_change, movement_type, source_document_type, source_document_id, transfer_line_id, created_by_user_id, notes, occurred_at_utc, created_at_utc, tenant_id) FROM stdin;
ecf13065-14fc-450b-ac71-b460671c1b33	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	10000.000000	OpeningBalance	StockAdjustment	13d57f59-a2f0-4d62-a96d-b82292a090b3	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-27 23:56:15.478667+02	2026-09-27 23:56:15.478667+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
3d3739eb-c310-4744-a7f5-5f3f2339ed00	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	10000.000000	PurchaseReceipt	Purchase	e0e5f410-12bc-42c0-93ac-366315fea946	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Purchase receipt	2026-09-27 23:57:25.802746+02	2026-09-27 23:57:25.802746+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
866e301e-7b32-4804-83d1-90655c8d51f0	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	-1000.000000	SalesShipment	Sale	4067bd4e-b9bf-4bc8-b54a-22ef99e65bc0	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Sales shipment	2026-09-27 23:58:10.056241+02	2026-09-27 23:58:10.056241+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
ab8e0741-d6a4-4a64-807a-e3a164ab19d6	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	15000.000000	OpeningBalance	StockAdjustment	0ac2d693-cf50-44cf-a901-0d032cee981b	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:04:30.807351+02	2026-09-28 00:04:30.807351+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
14bdd284-4799-4436-ae3e-70315defd721	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	OpeningBalance	StockAdjustment	33ab19c8-569d-4321-827b-02f4472d086c	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:08:03.418234+02	2026-09-28 00:08:03.418234+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
18258624-04c3-4ac3-ba1d-2deca2a98a60	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	OpeningBalance	StockAdjustment	0ee056d2-9e27-4820-be4c-b317db941c0f	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:08:07.209202+02	2026-09-28 00:08:07.209202+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
e3f7c0d9-710e-4eb4-8579-4bdda200f419	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	OpeningBalance	StockAdjustment	64314585-d359-4406-bcf4-268a22994165	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:08:08.786134+02	2026-09-28 00:08:08.786134+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
f2362a0c-8c5d-407c-9c42-41572c0bd512	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	OpeningBalance	StockAdjustment	c86b6338-333c-4ecb-91cd-66541a71ba24	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:08:11.142088+02	2026-09-28 00:08:11.142088+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
be3380df-0d77-4759-a427-88df7117738b	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	10.000000	OpeningBalance	StockAdjustment	c05b6ffb-92cb-4639-943b-03a8de0a0ecf	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:08:15.471654+02	2026-09-28 00:08:15.471654+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
7c05dccc-dec7-444c-968e-ec4fc68236f6	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	10.000000	OpeningBalance	StockAdjustment	8ea6aaf6-5645-42f6-9a58-aaf8e4660dbb	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:08:18.003645+02	2026-09-28 00:08:18.003645+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
f3121f8b-d993-43f5-b509-de938da7631e	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	OpeningBalance	StockAdjustment	9450966c-d263-4d36-9fd5-336685af09bd	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:15:33.691731+02	2026-09-28 00:15:33.691731+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
13f7a20e-8e60-42dc-ab56-55dd02cb3e0d	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	OpeningBalance	StockAdjustment	13138ee8-1234-4fd3-88d6-69fb7096bca2	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:15:37.278177+02	2026-09-28 00:15:37.278177+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
efc1571d-bb9b-4acd-91ad-c15fa8e24d02	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	10.000000	OpeningBalance	StockAdjustment	2e492b52-4850-496f-8096-0099b4c5117c	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Opening balance	2026-09-28 00:15:39.774272+02	2026-09-28 00:15:39.774272+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
71a3cc3b-9267-420a-b597-aa61edd4ae97	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	10000.000000	PurchaseReceipt	Purchase	48f6dad3-6d76-48b0-9b93-afac030634fb	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Purchase receipt	2026-09-28 00:16:37.45147+02	2026-09-28 00:16:37.45147+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
de297fc6-b878-4d2d-ac63-13da1891a112	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	-2500.000000	SalesShipment	Sale	3fbfc155-b5e8-4013-a149-6a186f6a10c7	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	Sales shipment	2026-09-28 00:17:28.15863+02	2026-09-28 00:17:28.15863+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
0bd3d6d5-036b-4b17-92e7-2161599cfb0f	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	2b16062e-7425-4704-bc4b-d339c60bd3d3	-1.000000	TransferDispatch	Transfer	25a3e047-3a88-49b8-9c41-b47ebcbee491	0c8ceeed-81d5-498a-a3ae-af931a675bb0	813a8227-bc81-4580-ba4e-598cb2bf2231	Transfer dispatch (source)	2026-09-29 11:06:35.741296+02	2026-09-29 11:06:35.741296+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
2417baa1-6082-4d89-9cd7-017de919eceb	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	06696022-fc7e-4b0c-910d-f80ebee212cc	1.000000	TransferDispatch	Transfer	25a3e047-3a88-49b8-9c41-b47ebcbee491	0c8ceeed-81d5-498a-a3ae-af931a675bb0	813a8227-bc81-4580-ba4e-598cb2bf2231	Transfer dispatch (in-transit)	2026-09-29 11:06:35.741296+02	2026-09-29 11:06:35.741296+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
6e810cd4-6327-414a-ae2c-00bcd2c6b6af	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	9403ba49-33c1-443f-963b-b0cc0b321d6b	06696022-fc7e-4b0c-910d-f80ebee212cc	-1.000000	TransferReceive	Transfer	25a3e047-3a88-49b8-9c41-b47ebcbee491	0c8ceeed-81d5-498a-a3ae-af931a675bb0	813a8227-bc81-4580-ba4e-598cb2bf2231	Transfer receive (leave in-transit)	2026-09-29 11:06:38.548023+02	2026-09-29 11:06:38.548023+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
9251ccec-1998-4dc8-8d69-b3f08f259e39	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	bae87cce-f597-4baa-8f26-158eecbdbeb2	1.000000	TransferReceive	Transfer	25a3e047-3a88-49b8-9c41-b47ebcbee491	0c8ceeed-81d5-498a-a3ae-af931a675bb0	813a8227-bc81-4580-ba4e-598cb2bf2231	Transfer receive (destination)	2026-09-29 11:06:38.548023+02	2026-09-29 11:06:38.548023+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
e4ee933e-8237-4e3c-be24-cec99f6a2ee2	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	10.000000	OpeningBalance	StockAdjustment	601b2323-a544-4e97-8592-28068abea11b	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Opening balance	2026-10-26 13:21:10.926479+02	2026-10-26 13:21:10.926479+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
43831afd-cfaf-4427-bab6-ea5bfd8f4262	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	50.000000	PurchaseReceipt	Purchase	40588805-f807-4548-bb45-4d502b6e6521	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Purchase receipt	2026-10-26 13:23:12.677646+02	2026-10-26 13:23:12.677646+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
f51566f3-e335-4367-bb19-de7ecd863187	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	-15.000000	SalesShipment	Sale	7619bc07-516c-48a2-9109-918d8f6b5f77	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Sales shipment	2026-10-26 13:23:21.355734+02	2026-10-26 13:23:21.355734+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
a46ab5aa-7423-48dc-8e8d-cbc108f7f92c	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	-10.000000	SalesShipment	Sale	6f8360a8-4e05-401d-9c8f-e937c254bd14	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	POS sale POS-20261026-641D1F90	2026-10-26 13:27:37.859029+02	2026-10-26 13:27:37.859029+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
8f99d74d-d031-4a91-bd87-8d6ac107f2d9	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	-1.000000	SalesShipment	Sale	5aefd3bd-f483-4538-a290-a34c251d5f91	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	POS sale POS-20261026-9C6A7781	2026-10-26 13:28:55.772715+02	2026-10-26 13:28:55.772715+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
02502d2e-0d37-4cb8-a527-0249decc95db	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	30.000000	PurchaseReceipt	Purchase	8d29e380-fe16-44f6-bcee-6c37ad836ca2	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Purchase receipt	2026-10-26 13:29:59.354044+02	2026-10-26 13:29:59.354044+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
8df5d052-67a2-400f-bde4-c52e763ba3e4	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	-1.000000	SalesShipment	Sale	a8957e96-02a1-4b4d-b1bd-3d6a5d92e01a	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	POS sale POS-20261026-2CA2ABBC	2026-10-26 21:41:03.292007+02	2026-10-26 21:41:03.292007+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
40b3271e-cd00-4a1d-beaf-04c75d958717	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	20.000000	OpeningBalance	StockAdjustment	773f8dfb-4df8-4854-8a1d-22f90cbc082e	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Opening balance	2026-10-29 10:30:08.454649+02	2026-10-29 10:30:08.454649+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
d46fefa7-092a-4f22-a2d4-651e38408d43	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	1.000000	PurchaseReceipt	Purchase	47c0d68a-60cc-4b99-936a-404c4d346696	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Purchase receipt	2026-10-29 10:31:02.898424+02	2026-10-29 10:31:02.898424+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
90ec0886-50fa-4ef0-8cda-ddff2ed4cf97	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	1.000000	PurchaseReceipt	Purchase	8d5b7beb-39f1-4339-99fd-e14d14972c56	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Purchase receipt	2026-10-29 10:31:25.502159+02	2026-10-29 10:31:25.502159+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
cae3f73a-db24-492a-bf04-b59bbb9d135d	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	10.000000	PurchaseReceipt	Purchase	615556e5-61b9-412d-9a6f-22bc2927eda4	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Purchase receipt	2026-10-29 10:32:08.811043+02	2026-10-29 10:32:08.811043+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
afcad9c8-13b3-4cb9-af10-d08b3d6ebd9f	0126728d-23b2-45bf-8065-cfd7455d4db0	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	-50.000000	SalesShipment	Sale	536fcb67-133c-4b7e-bfca-4aa3eac3b7cc	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Sales shipment	2026-10-29 10:32:42.897923+02	2026-10-29 10:32:42.897923+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
ab87694d-85fc-4c97-9ec7-ddc8da66bbe2	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	70.000000	PurchaseReceipt	Purchase	c3dc5abb-99c0-4eab-9635-dd0f6bb039da	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Purchase receipt	2026-10-29 11:33:51.731263+02	2026-10-29 11:33:51.731263+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
e8c8be60-fc60-4ac5-9112-cdfe7235a901	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	-55.000000	SalesShipment	Sale	7b92f2a7-bd1f-4e80-ae27-73410847037f	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Sales shipment	2026-10-29 11:34:22.917307+02	2026-10-29 11:34:22.917307+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
da664868-21a6-4612-aa3e-2ea9a9881f92	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	3c48cd3b-f770-41d4-8889-94f96cc3eaef	2117a1cb-b030-48a6-b3ac-802116e23052	50.000000	PurchaseReceipt	Purchase	fbef4059-14be-48b1-8edd-3adaaa644816	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Purchase receipt	2026-10-29 11:36:05.563013+02	2026-10-29 11:36:05.563013+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
825c7920-1568-44ed-a687-1c01630d582f	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	3c48cd3b-f770-41d4-8889-94f96cc3eaef	2117a1cb-b030-48a6-b3ac-802116e23052	-1.000000	SalesShipment	Sale	19a2b057-8df6-4e55-a5e3-5149200471b2	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	Sales shipment	2026-10-29 11:36:21.908055+02	2026-10-29 11:36:21.908055+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
8c3e9c0a-a665-4923-bebf-caddcfa64280	6bc4e0dc-01d7-4cff-a777-ea259e9a4e4d	bd357264-c8d1-472b-987d-575ac34ff384	004058ee-f0d7-417c-8817-ff95f6371754	-2.000000	SalesShipment	Sale	bb6c8fe8-4f2d-4ae3-8432-4cac9135a4ed	\N	b3b30173-470c-49f7-9991-8f4e0dbea3de	POS sale POS-20261029-C798DDFF	2026-10-29 12:55:44.700188+02	2026-10-29 12:55:44.700188+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: suppliers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.suppliers (id, code, name, contact_name, email, phone, notes, is_active, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
9364dd79-a87b-436d-a35d-ab3585ef58ac	15	??? ?????	\N	\N	\N	\N	t	2026-09-27 23:56:46.08318+02	2026-09-27 23:56:46.08318+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
9c13f046-f3e9-4a6d-85dc-5b0e40a31602	12	1delegator	\N	\N	\N	\N	t	2026-10-26 13:22:19.925587+02	2026-10-26 13:22:19.925587+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: tenant_change_counters; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.tenant_change_counters (tenant_id, last_change_version) FROM stdin;
\.


--
-- Data for Name: tenant_subscriptions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.tenant_subscriptions (id, plan_id, status, trial_ends_at_utc, current_period_start_utc, current_period_end_utc, external_reference, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
8e82b609-ea20-4279-aa18-947583dc9e8b	22222222-2222-2222-2222-222222222201	Trial	2026-10-27 23:00:18.52976+02	2026-09-27 23:00:18.52976+02	2026-10-27 23:00:18.52976+02	\N	2026-09-27 23:00:18.52976+02	2026-09-27 23:00:18.52976+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
df9191a8-d089-457a-b05e-351b953959bd	22222222-2222-2222-2222-222222222203	Active	\N	2026-09-27 23:07:36.431614+02	2026-10-27 23:07:36.431614+02	\N	2026-09-27 23:07:36.431614+02	2026-09-27 23:07:36.431614+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
\.


--
-- Data for Name: tenants; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.tenants (id, code, name, is_active, is_suspended, currency_code, time_zone_id, negative_stock_policy, tax_enabled, tax_name, tax_rate, max_offline_days, require_online_revalidation, created_at_utc, updated_at_utc, row_version) FROM stdin;
bd9cbe72-fe4c-4753-be04-d3040b554f67	123	???? ???? ???? 1	t	f	LYD	Africa/Tripoli	Prevent	f	\N	\N	7	f	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1
e534cf35-c3f3-47e1-92a4-ed14befb3e91	DEM2	demm	t	f	LYD	Africa/Tripoli	Prevent	f	\N	\N	7	f	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1
\.


--
-- Data for Name: transfer_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.transfer_lines (id, transfer_id, product_id, quantity_requested, quantity_received, notes, tenant_id) FROM stdin;
0c8ceeed-81d5-498a-a3ae-af931a675bb0	25a3e047-3a88-49b8-9c41-b47ebcbee491	2d05a15d-6a0e-4777-ba6a-fe4cbb3d4172	1.000000	1.000000	\N	e534cf35-c3f3-47e1-92a4-ed14befb3e91
\.


--
-- Data for Name: transfer_operations; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.transfer_operations (id, transfer_id, action, result_payload_json, processed_at_utc, tenant_id) FROM stdin;
\.


--
-- Data for Name: transfers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.transfers (id, source_warehouse_id, destination_warehouse_id, source_location_id, destination_location_id, status, notes, created_by_user_id, dispatch_operation_id, created_at_utc, updated_at_utc, dispatched_at_utc, completed_at_utc, row_version, tenant_id) FROM stdin;
25a3e047-3a88-49b8-9c41-b47ebcbee491	9403ba49-33c1-443f-963b-b0cc0b321d6b	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	2b16062e-7425-4704-bc4b-d339c60bd3d3	bae87cce-f597-4baa-8f26-158eecbdbeb2	Completed	\N	813a8227-bc81-4580-ba4e-598cb2bf2231	\N	2026-09-29 11:06:25.977804+02	2026-09-29 11:06:38.548023+02	2026-09-29 11:06:35.741296+02	2026-09-29 11:06:38.548023+02	3	e534cf35-c3f3-47e1-92a4-ed14befb3e91
\.


--
-- Data for Name: units_of_measure; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.units_of_measure (id, code, name, is_system, is_active, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
07980ada-6fed-4553-9a26-120a609735b7	L	Liter	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
53318045-6847-4213-8590-b7cff919d2ee	KG	Kilogram	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
783a6ad8-70ef-48c8-b33d-c04b25bb0209	M	Meter	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
dab2deb5-334b-4f13-b194-f636aa97759e	G	Gram	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
e56bd4e4-01fa-4e13-90bc-b097cc3f01d6	BOX	Box	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
ed090f18-3dfe-4a92-8b94-2e313adbd1ce	PIECE	Piece	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
f4c42b2a-9979-47ad-bc5a-7d8c10bfe947	CARTON	Carton	t	t	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
6230a443-b849-42cf-9879-f2754d312598	PIECE	Piece	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
865b0288-256f-4891-83e7-9d5cf6d921d5	BOX	Box	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
89a30d7d-7ba8-4bbe-9001-9403a7c20e8c	CARTON	Carton	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
a59f82f9-0537-4eee-8f25-c460bc13ae3e	G	Gram	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
c7ec4bd0-756d-4464-a669-e82e9aa83141	KG	Kilogram	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
c80283ba-aade-4d17-a44e-cabf8047ca67	M	Meter	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
d496ee06-6d70-4389-bf1b-5d3887ab2702	L	Liter	t	t	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
\.


--
-- Data for Name: user_roles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.user_roles (id, user_id, role_id, created_at_utc, tenant_id) FROM stdin;
65d2b18b-59b5-44f9-a60a-429804d75bb3	b3b30173-470c-49f7-9991-8f4e0dbea3de	9f913d3a-3fdf-4d0d-9892-c383b2aa7ba3	2026-09-27 23:00:17.328856+02	bd9cbe72-fe4c-4753-be04-d3040b554f67
9e5f19d6-188c-43dd-a9f7-d9111f0d04e5	813a8227-bc81-4580-ba4e-598cb2bf2231	0580eb73-8039-4acf-b62c-4e97d7021594	2026-09-27 23:07:36.140415+02	e534cf35-c3f3-47e1-92a4-ed14befb3e91
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.users (id, email, password_hash, display_name, is_active, is_disabled, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
b3b30173-470c-49f7-9991-8f4e0dbea3de	platform@warehouse.local	AQAAAAIAAYagAAAAEJFU061qbl59V3/rWuFxUf4gFhtZLsnT21OiE1zfp/lJgRn6BARHb/vzesCU7ZAZvg==	???? ??????	t	f	2026-09-27 23:00:17.328856+02	2026-09-27 23:00:17.328856+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
813a8227-bc81-4580-ba4e-598cb2bf2231	aboughoufa2014@gmail.com	AQAAAAIAAYagAAAAEHpXnh8+0pJNLyfnMMUMjPWH9gqh59o+qk3SkjBnFZumuAhkS+wX0UxOGWluv9JucA==	???? ??????	t	f	2026-09-27 23:07:36.140415+02	2026-09-27 23:07:36.140415+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
\.


--
-- Data for Name: warehouse_locations; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.warehouse_locations (id, warehouse_id, code, name, kind, is_default, is_active, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
06696022-fc7e-4b0c-910d-f80ebee212cc	9403ba49-33c1-443f-963b-b0cc0b321d6b	IN_TRANSIT	In Transit	InTransit	f	t	2026-09-27 23:53:43.807171+02	2026-09-27 23:53:43.807171+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
2b16062e-7425-4704-bc4b-d339c60bd3d3	9403ba49-33c1-443f-963b-b0cc0b321d6b	GENERAL	General	System	t	t	2026-09-27 23:53:43.807171+02	2026-09-27 23:53:43.807171+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
2f5f4348-4479-4597-8026-846e465c3829	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	IN_TRANSIT	In Transit	InTransit	f	t	2026-09-28 00:04:18.104343+02	2026-09-28 00:04:18.104343+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
bae87cce-f597-4baa-8f26-158eecbdbeb2	4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	GENERAL	General	System	t	t	2026-09-28 00:04:18.104343+02	2026-09-28 00:04:18.104343+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
004058ee-f0d7-417c-8817-ff95f6371754	bd357264-c8d1-472b-987d-575ac34ff384	GENERAL	General	System	t	t	2026-10-26 13:21:06.395256+02	2026-10-26 13:21:06.395256+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
d91fe100-637a-434b-8629-0ce77923e53f	bd357264-c8d1-472b-987d-575ac34ff384	IN_TRANSIT	In Transit	InTransit	f	t	2026-10-26 13:21:06.395256+02	2026-10-26 13:21:06.395256+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
2117a1cb-b030-48a6-b3ac-802116e23052	3c48cd3b-f770-41d4-8889-94f96cc3eaef	GENERAL	General	System	t	t	2026-10-29 10:59:09.469046+02	2026-10-29 10:59:09.469046+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
374910c0-affc-4351-b8f8-273e2f24a7f4	3c48cd3b-f770-41d4-8889-94f96cc3eaef	IN_TRANSIT	In Transit	InTransit	f	t	2026-10-29 10:59:09.469046+02	2026-10-29 10:59:09.469046+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Data for Name: warehouses; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.warehouses (id, code, name, is_active, created_at_utc, updated_at_utc, row_version, tenant_id) FROM stdin;
9403ba49-33c1-443f-963b-b0cc0b321d6b	1	???? ?????	t	2026-09-27 23:53:43.807171+02	2026-09-27 23:53:43.807171+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
4ee02d7f-c51a-4cef-8a77-7ad88b7bbe6d	15	???? ????	t	2026-09-28 00:04:18.104343+02	2026-09-28 00:04:18.104343+02	1	e534cf35-c3f3-47e1-92a4-ed14befb3e91
bd357264-c8d1-472b-987d-575ac34ff384	12	main	t	2026-10-26 13:21:06.395256+02	2026-10-26 13:21:06.395256+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
3c48cd3b-f770-41d4-8889-94f96cc3eaef	14	main1	t	2026-10-29 10:59:09.469046+02	2026-10-29 10:59:09.469046+02	1	bd9cbe72-fe4c-4753-be04-d3040b554f67
\.


--
-- Name: __EFMigrationsHistory pk___ef_migrations_history; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id);


--
-- Name: audit_logs pk_audit_logs; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT pk_audit_logs PRIMARY KEY (id);


--
-- Name: cash_movements pk_cash_movements; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cash_movements
    ADD CONSTRAINT pk_cash_movements PRIMARY KEY (id);


--
-- Name: cash_shifts pk_cash_shifts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cash_shifts
    ADD CONSTRAINT pk_cash_shifts PRIMARY KEY (id);


--
-- Name: change_log pk_change_log; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.change_log
    ADD CONSTRAINT pk_change_log PRIMARY KEY (id);


--
-- Name: customers pk_customers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT pk_customers PRIMARY KEY (id);


--
-- Name: devices pk_devices; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.devices
    ADD CONSTRAINT pk_devices PRIMARY KEY (id);


--
-- Name: permissions pk_permissions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.permissions
    ADD CONSTRAINT pk_permissions PRIMARY KEY (id);


--
-- Name: plans pk_plans; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.plans
    ADD CONSTRAINT pk_plans PRIMARY KEY (id);


--
-- Name: platform_operators pk_platform_operators; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.platform_operators
    ADD CONSTRAINT pk_platform_operators PRIMARY KEY (id);


--
-- Name: pos_inventory_conflicts pk_pos_inventory_conflicts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_inventory_conflicts
    ADD CONSTRAINT pk_pos_inventory_conflicts PRIMARY KEY (id);


--
-- Name: pos_payments pk_pos_payments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_payments
    ADD CONSTRAINT pk_pos_payments PRIMARY KEY (id);


--
-- Name: pos_sale_lines pk_pos_sale_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_sale_lines
    ADD CONSTRAINT pk_pos_sale_lines PRIMARY KEY (id);


--
-- Name: pos_sales pk_pos_sales; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_sales
    ADD CONSTRAINT pk_pos_sales PRIMARY KEY (id);


--
-- Name: price_lists pk_price_lists; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.price_lists
    ADD CONSTRAINT pk_price_lists PRIMARY KEY (id);


--
-- Name: processed_sync_operations pk_processed_sync_operations; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.processed_sync_operations
    ADD CONSTRAINT pk_processed_sync_operations PRIMARY KEY (id);


--
-- Name: product_barcodes pk_product_barcodes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_barcodes
    ADD CONSTRAINT pk_product_barcodes PRIMARY KEY (id);


--
-- Name: product_categories pk_product_categories; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_categories
    ADD CONSTRAINT pk_product_categories PRIMARY KEY (id);


--
-- Name: product_prices pk_product_prices; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_prices
    ADD CONSTRAINT pk_product_prices PRIMARY KEY (id);


--
-- Name: products pk_products; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT pk_products PRIMARY KEY (id);


--
-- Name: purchase_operations pk_purchase_operations; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_operations
    ADD CONSTRAINT pk_purchase_operations PRIMARY KEY (id);


--
-- Name: purchase_order_lines pk_purchase_order_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_order_lines
    ADD CONSTRAINT pk_purchase_order_lines PRIMARY KEY (id);


--
-- Name: purchase_orders pk_purchase_orders; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT pk_purchase_orders PRIMARY KEY (id);


--
-- Name: purchase_receipt_lines pk_purchase_receipt_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipt_lines
    ADD CONSTRAINT pk_purchase_receipt_lines PRIMARY KEY (id);


--
-- Name: purchase_receipts pk_purchase_receipts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipts
    ADD CONSTRAINT pk_purchase_receipts PRIMARY KEY (id);


--
-- Name: purchase_return_lines pk_purchase_return_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_return_lines
    ADD CONSTRAINT pk_purchase_return_lines PRIMARY KEY (id);


--
-- Name: purchase_returns pk_purchase_returns; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_returns
    ADD CONSTRAINT pk_purchase_returns PRIMARY KEY (id);


--
-- Name: refresh_tokens pk_refresh_tokens; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT pk_refresh_tokens PRIMARY KEY (id);


--
-- Name: restaurant_open_orders pk_restaurant_open_orders; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.restaurant_open_orders
    ADD CONSTRAINT pk_restaurant_open_orders PRIMARY KEY (id);


--
-- Name: restaurant_tables pk_restaurant_tables; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.restaurant_tables
    ADD CONSTRAINT pk_restaurant_tables PRIMARY KEY (id);


--
-- Name: role_permissions pk_role_permissions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.role_permissions
    ADD CONSTRAINT pk_role_permissions PRIMARY KEY (id);


--
-- Name: roles pk_roles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT pk_roles PRIMARY KEY (id);


--
-- Name: sales_operations pk_sales_operations; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_operations
    ADD CONSTRAINT pk_sales_operations PRIMARY KEY (id);


--
-- Name: sales_order_lines pk_sales_order_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_order_lines
    ADD CONSTRAINT pk_sales_order_lines PRIMARY KEY (id);


--
-- Name: sales_orders pk_sales_orders; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_orders
    ADD CONSTRAINT pk_sales_orders PRIMARY KEY (id);


--
-- Name: sales_return_lines pk_sales_return_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_return_lines
    ADD CONSTRAINT pk_sales_return_lines PRIMARY KEY (id);


--
-- Name: sales_returns pk_sales_returns; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_returns
    ADD CONSTRAINT pk_sales_returns PRIMARY KEY (id);


--
-- Name: sales_shipment_lines pk_sales_shipment_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipment_lines
    ADD CONSTRAINT pk_sales_shipment_lines PRIMARY KEY (id);


--
-- Name: sales_shipments pk_sales_shipments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipments
    ADD CONSTRAINT pk_sales_shipments PRIMARY KEY (id);


--
-- Name: stock_adjustment_lines pk_stock_adjustment_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_adjustment_lines
    ADD CONSTRAINT pk_stock_adjustment_lines PRIMARY KEY (id);


--
-- Name: stock_adjustments pk_stock_adjustments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_adjustments
    ADD CONSTRAINT pk_stock_adjustments PRIMARY KEY (id);


--
-- Name: stock_balances pk_stock_balances; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_balances
    ADD CONSTRAINT pk_stock_balances PRIMARY KEY (id);


--
-- Name: stock_count_lines pk_stock_count_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_count_lines
    ADD CONSTRAINT pk_stock_count_lines PRIMARY KEY (id);


--
-- Name: stock_counts pk_stock_counts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_counts
    ADD CONSTRAINT pk_stock_counts PRIMARY KEY (id);


--
-- Name: stock_movements pk_stock_movements; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_movements
    ADD CONSTRAINT pk_stock_movements PRIMARY KEY (id);


--
-- Name: suppliers pk_suppliers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.suppliers
    ADD CONSTRAINT pk_suppliers PRIMARY KEY (id);


--
-- Name: tenant_change_counters pk_tenant_change_counters; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tenant_change_counters
    ADD CONSTRAINT pk_tenant_change_counters PRIMARY KEY (tenant_id);


--
-- Name: tenant_subscriptions pk_tenant_subscriptions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tenant_subscriptions
    ADD CONSTRAINT pk_tenant_subscriptions PRIMARY KEY (id);


--
-- Name: tenants pk_tenants; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tenants
    ADD CONSTRAINT pk_tenants PRIMARY KEY (id);


--
-- Name: transfer_lines pk_transfer_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfer_lines
    ADD CONSTRAINT pk_transfer_lines PRIMARY KEY (id);


--
-- Name: transfer_operations pk_transfer_operations; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfer_operations
    ADD CONSTRAINT pk_transfer_operations PRIMARY KEY (id);


--
-- Name: transfers pk_transfers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfers
    ADD CONSTRAINT pk_transfers PRIMARY KEY (id);


--
-- Name: units_of_measure pk_units_of_measure; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.units_of_measure
    ADD CONSTRAINT pk_units_of_measure PRIMARY KEY (id);


--
-- Name: user_roles pk_user_roles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT pk_user_roles PRIMARY KEY (id);


--
-- Name: users pk_users; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT pk_users PRIMARY KEY (id);


--
-- Name: warehouse_locations pk_warehouse_locations; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_locations
    ADD CONSTRAINT pk_warehouse_locations PRIMARY KEY (id);


--
-- Name: warehouses pk_warehouses; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouses
    ADD CONSTRAINT pk_warehouses PRIMARY KEY (id);


--
-- Name: ix_audit_logs_correlation_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_audit_logs_correlation_id ON public.audit_logs USING btree (correlation_id);


--
-- Name: ix_audit_logs_device_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_audit_logs_device_id ON public.audit_logs USING btree (device_id);


--
-- Name: ix_audit_logs_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_audit_logs_tenant_id ON public.audit_logs USING btree (tenant_id);


--
-- Name: ix_audit_logs_tenant_id_entity_type_entity_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_audit_logs_tenant_id_entity_type_entity_id ON public.audit_logs USING btree (tenant_id, entity_type, entity_id);


--
-- Name: ix_audit_logs_tenant_id_occurred_at_utc; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_audit_logs_tenant_id_occurred_at_utc ON public.audit_logs USING btree (tenant_id, occurred_at_utc);


--
-- Name: ix_audit_logs_tenant_id_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_audit_logs_tenant_id_user_id ON public.audit_logs USING btree (tenant_id, user_id);


--
-- Name: ix_audit_logs_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_audit_logs_user_id ON public.audit_logs USING btree (user_id);


--
-- Name: ix_cash_movements_cash_shift_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_cash_movements_cash_shift_id ON public.cash_movements USING btree (cash_shift_id);


--
-- Name: ix_cash_movements_tenant_id_cash_shift_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_cash_movements_tenant_id_cash_shift_id ON public.cash_movements USING btree (tenant_id, cash_shift_id);


--
-- Name: ix_cash_shifts_tenant_id_warehouse_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_cash_shifts_tenant_id_warehouse_id_status ON public.cash_shifts USING btree (tenant_id, warehouse_id, status);


--
-- Name: ix_change_log_device_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_change_log_device_id ON public.change_log USING btree (device_id);


--
-- Name: ix_change_log_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_change_log_tenant_id ON public.change_log USING btree (tenant_id);


--
-- Name: ix_change_log_tenant_id_change_version; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_change_log_tenant_id_change_version ON public.change_log USING btree (tenant_id, change_version);


--
-- Name: ix_change_log_tenant_id_entity_type_entity_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_change_log_tenant_id_entity_type_entity_id ON public.change_log USING btree (tenant_id, entity_type, entity_id);


--
-- Name: ix_change_log_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_change_log_user_id ON public.change_log USING btree (user_id);


--
-- Name: ix_customers_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_customers_tenant_id ON public.customers USING btree (tenant_id);


--
-- Name: ix_customers_tenant_id_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_customers_tenant_id_code ON public.customers USING btree (tenant_id, code);


--
-- Name: ix_customers_tenant_id_is_active; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_customers_tenant_id_is_active ON public.customers USING btree (tenant_id, is_active);


--
-- Name: ix_devices_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_devices_tenant_id ON public.devices USING btree (tenant_id);


--
-- Name: ix_devices_tenant_id_device_key; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_devices_tenant_id_device_key ON public.devices USING btree (tenant_id, device_key);


--
-- Name: ix_devices_tenant_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_devices_tenant_id_status ON public.devices USING btree (tenant_id, status);


--
-- Name: ix_devices_tenant_id_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_devices_tenant_id_user_id ON public.devices USING btree (tenant_id, user_id);


--
-- Name: ix_devices_tenant_id_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_devices_tenant_id_warehouse_id ON public.devices USING btree (tenant_id, warehouse_id);


--
-- Name: ix_devices_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_devices_user_id ON public.devices USING btree (user_id);


--
-- Name: ix_devices_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_devices_warehouse_id ON public.devices USING btree (warehouse_id);


--
-- Name: ix_permissions_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_permissions_code ON public.permissions USING btree (code);


--
-- Name: ix_plans_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_plans_code ON public.plans USING btree (code);


--
-- Name: ix_platform_operators_email; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_platform_operators_email ON public.platform_operators USING btree (email);


--
-- Name: ix_pos_inventory_conflicts_tenant_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pos_inventory_conflicts_tenant_id_status ON public.pos_inventory_conflicts USING btree (tenant_id, status);


--
-- Name: ix_pos_payments_pos_sale_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pos_payments_pos_sale_id ON public.pos_payments USING btree (pos_sale_id);


--
-- Name: ix_pos_payments_tenant_id_pos_sale_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pos_payments_tenant_id_pos_sale_id ON public.pos_payments USING btree (tenant_id, pos_sale_id);


--
-- Name: ix_pos_sale_lines_pos_sale_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pos_sale_lines_pos_sale_id ON public.pos_sale_lines USING btree (pos_sale_id);


--
-- Name: ix_pos_sale_lines_tenant_id_pos_sale_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pos_sale_lines_tenant_id_pos_sale_id ON public.pos_sale_lines USING btree (tenant_id, pos_sale_id);


--
-- Name: ix_pos_sales_tenant_id_client_command_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_pos_sales_tenant_id_client_command_id ON public.pos_sales USING btree (tenant_id, client_command_id);


--
-- Name: ix_pos_sales_tenant_id_receipt_number; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pos_sales_tenant_id_receipt_number ON public.pos_sales USING btree (tenant_id, receipt_number);


--
-- Name: ix_pos_sales_tenant_id_warehouse_id_sold_at_utc; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_pos_sales_tenant_id_warehouse_id_sold_at_utc ON public.pos_sales USING btree (tenant_id, warehouse_id, sold_at_utc);


--
-- Name: ix_price_lists_tenant_id_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_price_lists_tenant_id_code ON public.price_lists USING btree (tenant_id, code);


--
-- Name: ix_processed_sync_operations_device_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_processed_sync_operations_device_id ON public.processed_sync_operations USING btree (device_id);


--
-- Name: ix_processed_sync_operations_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_processed_sync_operations_tenant_id ON public.processed_sync_operations USING btree (tenant_id);


--
-- Name: ix_processed_sync_operations_tenant_id_created_at_utc; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_processed_sync_operations_tenant_id_created_at_utc ON public.processed_sync_operations USING btree (tenant_id, created_at_utc);


--
-- Name: ix_processed_sync_operations_tenant_id_device_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_processed_sync_operations_tenant_id_device_id ON public.processed_sync_operations USING btree (tenant_id, device_id);


--
-- Name: ix_processed_sync_operations_tenant_id_entity_type_entity_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_processed_sync_operations_tenant_id_entity_type_entity_id ON public.processed_sync_operations USING btree (tenant_id, entity_type, entity_id);


--
-- Name: ix_processed_sync_operations_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_processed_sync_operations_user_id ON public.processed_sync_operations USING btree (user_id);


--
-- Name: ix_product_barcodes_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_product_barcodes_product_id ON public.product_barcodes USING btree (product_id);


--
-- Name: ix_product_barcodes_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_product_barcodes_tenant_id ON public.product_barcodes USING btree (tenant_id);


--
-- Name: ix_product_barcodes_tenant_id_barcode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_product_barcodes_tenant_id_barcode ON public.product_barcodes USING btree (tenant_id, barcode);


--
-- Name: ix_product_barcodes_tenant_id_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_product_barcodes_tenant_id_product_id ON public.product_barcodes USING btree (tenant_id, product_id);


--
-- Name: ix_product_categories_parent_category_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_product_categories_parent_category_id ON public.product_categories USING btree (parent_category_id);


--
-- Name: ix_product_categories_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_product_categories_tenant_id ON public.product_categories USING btree (tenant_id);


--
-- Name: ix_product_categories_tenant_id_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_product_categories_tenant_id_code ON public.product_categories USING btree (tenant_id, code);


--
-- Name: ix_product_categories_tenant_id_is_active; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_product_categories_tenant_id_is_active ON public.product_categories USING btree (tenant_id, is_active);


--
-- Name: ix_product_prices_price_list_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_product_prices_price_list_id ON public.product_prices USING btree (price_list_id);


--
-- Name: ix_product_prices_tenant_id_price_list_id_product_id_min_quant; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_product_prices_tenant_id_price_list_id_product_id_min_quant ON public.product_prices USING btree (tenant_id, price_list_id, product_id, min_quantity);


--
-- Name: ix_products_category_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_products_category_id ON public.products USING btree (category_id);


--
-- Name: ix_products_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_products_tenant_id ON public.products USING btree (tenant_id);


--
-- Name: ix_products_tenant_id_category_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_products_tenant_id_category_id ON public.products USING btree (tenant_id, category_id);


--
-- Name: ix_products_tenant_id_is_active; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_products_tenant_id_is_active ON public.products USING btree (tenant_id, is_active);


--
-- Name: ix_products_tenant_id_name; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_products_tenant_id_name ON public.products USING btree (tenant_id, name);


--
-- Name: ix_products_tenant_id_sku; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_products_tenant_id_sku ON public.products USING btree (tenant_id, sku);


--
-- Name: ix_products_unit_of_measure_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_products_unit_of_measure_id ON public.products USING btree (unit_of_measure_id);


--
-- Name: ix_purchase_operations_purchase_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_operations_purchase_order_id ON public.purchase_operations USING btree (purchase_order_id);


--
-- Name: ix_purchase_operations_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_operations_tenant_id ON public.purchase_operations USING btree (tenant_id);


--
-- Name: ix_purchase_operations_tenant_id_purchase_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_operations_tenant_id_purchase_order_id ON public.purchase_operations USING btree (tenant_id, purchase_order_id);


--
-- Name: ix_purchase_order_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_order_lines_product_id ON public.purchase_order_lines USING btree (product_id);


--
-- Name: ix_purchase_order_lines_purchase_order_id_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_purchase_order_lines_purchase_order_id_product_id ON public.purchase_order_lines USING btree (purchase_order_id, product_id);


--
-- Name: ix_purchase_order_lines_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_order_lines_tenant_id ON public.purchase_order_lines USING btree (tenant_id);


--
-- Name: ix_purchase_orders_created_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_orders_created_by_user_id ON public.purchase_orders USING btree (created_by_user_id);


--
-- Name: ix_purchase_orders_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_orders_location_id ON public.purchase_orders USING btree (location_id);


--
-- Name: ix_purchase_orders_supplier_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_orders_supplier_id ON public.purchase_orders USING btree (supplier_id);


--
-- Name: ix_purchase_orders_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_orders_tenant_id ON public.purchase_orders USING btree (tenant_id);


--
-- Name: ix_purchase_orders_tenant_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_orders_tenant_id_status ON public.purchase_orders USING btree (tenant_id, status);


--
-- Name: ix_purchase_orders_tenant_id_supplier_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_orders_tenant_id_supplier_id ON public.purchase_orders USING btree (tenant_id, supplier_id);


--
-- Name: ix_purchase_orders_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_orders_warehouse_id ON public.purchase_orders USING btree (warehouse_id);


--
-- Name: ix_purchase_receipt_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_receipt_lines_product_id ON public.purchase_receipt_lines USING btree (product_id);


--
-- Name: ix_purchase_receipt_lines_purchase_order_line_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_receipt_lines_purchase_order_line_id ON public.purchase_receipt_lines USING btree (purchase_order_line_id);


--
-- Name: ix_purchase_receipt_lines_purchase_receipt_id_purchase_order_li; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_purchase_receipt_lines_purchase_receipt_id_purchase_order_li ON public.purchase_receipt_lines USING btree (purchase_receipt_id, purchase_order_line_id);


--
-- Name: ix_purchase_receipt_lines_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_receipt_lines_tenant_id ON public.purchase_receipt_lines USING btree (tenant_id);


--
-- Name: ix_purchase_receipts_created_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_receipts_created_by_user_id ON public.purchase_receipts USING btree (created_by_user_id);


--
-- Name: ix_purchase_receipts_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_receipts_location_id ON public.purchase_receipts USING btree (location_id);


--
-- Name: ix_purchase_receipts_operation_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_purchase_receipts_operation_id ON public.purchase_receipts USING btree (operation_id);


--
-- Name: ix_purchase_receipts_purchase_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_receipts_purchase_order_id ON public.purchase_receipts USING btree (purchase_order_id);


--
-- Name: ix_purchase_receipts_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_receipts_tenant_id ON public.purchase_receipts USING btree (tenant_id);


--
-- Name: ix_purchase_receipts_tenant_id_purchase_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_receipts_tenant_id_purchase_order_id ON public.purchase_receipts USING btree (tenant_id, purchase_order_id);


--
-- Name: ix_purchase_receipts_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_receipts_warehouse_id ON public.purchase_receipts USING btree (warehouse_id);


--
-- Name: ix_purchase_return_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_return_lines_product_id ON public.purchase_return_lines USING btree (product_id);


--
-- Name: ix_purchase_return_lines_purchase_order_line_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_return_lines_purchase_order_line_id ON public.purchase_return_lines USING btree (purchase_order_line_id);


--
-- Name: ix_purchase_return_lines_purchase_return_id_purchase_order_line; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_purchase_return_lines_purchase_return_id_purchase_order_line ON public.purchase_return_lines USING btree (purchase_return_id, purchase_order_line_id);


--
-- Name: ix_purchase_return_lines_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_return_lines_tenant_id ON public.purchase_return_lines USING btree (tenant_id);


--
-- Name: ix_purchase_returns_created_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_returns_created_by_user_id ON public.purchase_returns USING btree (created_by_user_id);


--
-- Name: ix_purchase_returns_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_returns_location_id ON public.purchase_returns USING btree (location_id);


--
-- Name: ix_purchase_returns_operation_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_purchase_returns_operation_id ON public.purchase_returns USING btree (operation_id);


--
-- Name: ix_purchase_returns_purchase_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_returns_purchase_order_id ON public.purchase_returns USING btree (purchase_order_id);


--
-- Name: ix_purchase_returns_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_returns_tenant_id ON public.purchase_returns USING btree (tenant_id);


--
-- Name: ix_purchase_returns_tenant_id_purchase_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_returns_tenant_id_purchase_order_id ON public.purchase_returns USING btree (tenant_id, purchase_order_id);


--
-- Name: ix_purchase_returns_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_purchase_returns_warehouse_id ON public.purchase_returns USING btree (warehouse_id);


--
-- Name: ix_refresh_tokens_device_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_refresh_tokens_device_id ON public.refresh_tokens USING btree (device_id);


--
-- Name: ix_refresh_tokens_family_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_refresh_tokens_family_id ON public.refresh_tokens USING btree (family_id);


--
-- Name: ix_refresh_tokens_replaced_by_token_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_refresh_tokens_replaced_by_token_id ON public.refresh_tokens USING btree (replaced_by_token_id);


--
-- Name: ix_refresh_tokens_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_refresh_tokens_tenant_id ON public.refresh_tokens USING btree (tenant_id);


--
-- Name: ix_refresh_tokens_tenant_id_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_refresh_tokens_tenant_id_user_id ON public.refresh_tokens USING btree (tenant_id, user_id);


--
-- Name: ix_refresh_tokens_token_hash; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_refresh_tokens_token_hash ON public.refresh_tokens USING btree (token_hash);


--
-- Name: ix_refresh_tokens_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_refresh_tokens_user_id ON public.refresh_tokens USING btree (user_id);


--
-- Name: ix_restaurant_open_orders_table_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_restaurant_open_orders_table_id ON public.restaurant_open_orders USING btree (table_id);


--
-- Name: ix_restaurant_open_orders_tenant_id_warehouse_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_restaurant_open_orders_tenant_id_warehouse_id_status ON public.restaurant_open_orders USING btree (tenant_id, warehouse_id, status);


--
-- Name: ix_restaurant_tables_tenant_id_warehouse_id_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_restaurant_tables_tenant_id_warehouse_id_code ON public.restaurant_tables USING btree (tenant_id, warehouse_id, code);


--
-- Name: ix_role_permissions_permission_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_role_permissions_permission_id ON public.role_permissions USING btree (permission_id);


--
-- Name: ix_role_permissions_role_id_permission_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_role_permissions_role_id_permission_id ON public.role_permissions USING btree (role_id, permission_id);


--
-- Name: ix_role_permissions_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_role_permissions_tenant_id ON public.role_permissions USING btree (tenant_id);


--
-- Name: ix_role_permissions_tenant_id_role_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_role_permissions_tenant_id_role_id ON public.role_permissions USING btree (tenant_id, role_id);


--
-- Name: ix_roles_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_roles_tenant_id ON public.roles USING btree (tenant_id);


--
-- Name: ix_roles_tenant_id_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_roles_tenant_id_code ON public.roles USING btree (tenant_id, code);


--
-- Name: ix_roles_tenant_id_name; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_roles_tenant_id_name ON public.roles USING btree (tenant_id, name);


--
-- Name: ix_sales_operations_sales_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_operations_sales_order_id ON public.sales_operations USING btree (sales_order_id);


--
-- Name: ix_sales_operations_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_operations_tenant_id ON public.sales_operations USING btree (tenant_id);


--
-- Name: ix_sales_operations_tenant_id_sales_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_operations_tenant_id_sales_order_id ON public.sales_operations USING btree (tenant_id, sales_order_id);


--
-- Name: ix_sales_order_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_order_lines_product_id ON public.sales_order_lines USING btree (product_id);


--
-- Name: ix_sales_order_lines_sales_order_id_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_sales_order_lines_sales_order_id_product_id ON public.sales_order_lines USING btree (sales_order_id, product_id);


--
-- Name: ix_sales_order_lines_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_order_lines_tenant_id ON public.sales_order_lines USING btree (tenant_id);


--
-- Name: ix_sales_orders_created_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_orders_created_by_user_id ON public.sales_orders USING btree (created_by_user_id);


--
-- Name: ix_sales_orders_customer_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_orders_customer_id ON public.sales_orders USING btree (customer_id);


--
-- Name: ix_sales_orders_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_orders_location_id ON public.sales_orders USING btree (location_id);


--
-- Name: ix_sales_orders_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_orders_tenant_id ON public.sales_orders USING btree (tenant_id);


--
-- Name: ix_sales_orders_tenant_id_customer_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_orders_tenant_id_customer_id ON public.sales_orders USING btree (tenant_id, customer_id);


--
-- Name: ix_sales_orders_tenant_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_orders_tenant_id_status ON public.sales_orders USING btree (tenant_id, status);


--
-- Name: ix_sales_orders_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_orders_warehouse_id ON public.sales_orders USING btree (warehouse_id);


--
-- Name: ix_sales_return_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_return_lines_product_id ON public.sales_return_lines USING btree (product_id);


--
-- Name: ix_sales_return_lines_sales_order_line_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_return_lines_sales_order_line_id ON public.sales_return_lines USING btree (sales_order_line_id);


--
-- Name: ix_sales_return_lines_sales_return_id_sales_order_line_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_sales_return_lines_sales_return_id_sales_order_line_id ON public.sales_return_lines USING btree (sales_return_id, sales_order_line_id);


--
-- Name: ix_sales_return_lines_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_return_lines_tenant_id ON public.sales_return_lines USING btree (tenant_id);


--
-- Name: ix_sales_returns_created_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_returns_created_by_user_id ON public.sales_returns USING btree (created_by_user_id);


--
-- Name: ix_sales_returns_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_returns_location_id ON public.sales_returns USING btree (location_id);


--
-- Name: ix_sales_returns_operation_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_sales_returns_operation_id ON public.sales_returns USING btree (operation_id);


--
-- Name: ix_sales_returns_sales_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_returns_sales_order_id ON public.sales_returns USING btree (sales_order_id);


--
-- Name: ix_sales_returns_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_returns_tenant_id ON public.sales_returns USING btree (tenant_id);


--
-- Name: ix_sales_returns_tenant_id_sales_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_returns_tenant_id_sales_order_id ON public.sales_returns USING btree (tenant_id, sales_order_id);


--
-- Name: ix_sales_returns_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_returns_warehouse_id ON public.sales_returns USING btree (warehouse_id);


--
-- Name: ix_sales_shipment_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_shipment_lines_product_id ON public.sales_shipment_lines USING btree (product_id);


--
-- Name: ix_sales_shipment_lines_sales_order_line_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_shipment_lines_sales_order_line_id ON public.sales_shipment_lines USING btree (sales_order_line_id);


--
-- Name: ix_sales_shipment_lines_sales_shipment_id_sales_order_line_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_sales_shipment_lines_sales_shipment_id_sales_order_line_id ON public.sales_shipment_lines USING btree (sales_shipment_id, sales_order_line_id);


--
-- Name: ix_sales_shipment_lines_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_shipment_lines_tenant_id ON public.sales_shipment_lines USING btree (tenant_id);


--
-- Name: ix_sales_shipments_created_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_shipments_created_by_user_id ON public.sales_shipments USING btree (created_by_user_id);


--
-- Name: ix_sales_shipments_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_shipments_location_id ON public.sales_shipments USING btree (location_id);


--
-- Name: ix_sales_shipments_operation_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_sales_shipments_operation_id ON public.sales_shipments USING btree (operation_id);


--
-- Name: ix_sales_shipments_sales_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_shipments_sales_order_id ON public.sales_shipments USING btree (sales_order_id);


--
-- Name: ix_sales_shipments_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_shipments_tenant_id ON public.sales_shipments USING btree (tenant_id);


--
-- Name: ix_sales_shipments_tenant_id_sales_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_shipments_tenant_id_sales_order_id ON public.sales_shipments USING btree (tenant_id, sales_order_id);


--
-- Name: ix_sales_shipments_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_sales_shipments_warehouse_id ON public.sales_shipments USING btree (warehouse_id);


--
-- Name: ix_stock_adjustment_lines_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_adjustment_lines_location_id ON public.stock_adjustment_lines USING btree (location_id);


--
-- Name: ix_stock_adjustment_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_adjustment_lines_product_id ON public.stock_adjustment_lines USING btree (product_id);


--
-- Name: ix_stock_adjustment_lines_stock_adjustment_id_product_id_locati; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_stock_adjustment_lines_stock_adjustment_id_product_id_locati ON public.stock_adjustment_lines USING btree (stock_adjustment_id, product_id, location_id);


--
-- Name: ix_stock_adjustment_lines_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_adjustment_lines_tenant_id ON public.stock_adjustment_lines USING btree (tenant_id);


--
-- Name: ix_stock_adjustments_created_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_adjustments_created_by_user_id ON public.stock_adjustments USING btree (created_by_user_id);


--
-- Name: ix_stock_adjustments_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_adjustments_tenant_id ON public.stock_adjustments USING btree (tenant_id);


--
-- Name: ix_stock_adjustments_tenant_id_warehouse_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_adjustments_tenant_id_warehouse_id_status ON public.stock_adjustments USING btree (tenant_id, warehouse_id, status);


--
-- Name: ix_stock_adjustments_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_adjustments_warehouse_id ON public.stock_adjustments USING btree (warehouse_id);


--
-- Name: ix_stock_balances_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_balances_location_id ON public.stock_balances USING btree (location_id);


--
-- Name: ix_stock_balances_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_balances_product_id ON public.stock_balances USING btree (product_id);


--
-- Name: ix_stock_balances_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_balances_tenant_id ON public.stock_balances USING btree (tenant_id);


--
-- Name: ix_stock_balances_tenant_id_product_id_warehouse_id_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_stock_balances_tenant_id_product_id_warehouse_id_location_id ON public.stock_balances USING btree (tenant_id, product_id, warehouse_id, location_id);


--
-- Name: ix_stock_balances_tenant_id_warehouse_id_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_balances_tenant_id_warehouse_id_product_id ON public.stock_balances USING btree (tenant_id, warehouse_id, product_id);


--
-- Name: ix_stock_balances_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_balances_warehouse_id ON public.stock_balances USING btree (warehouse_id);


--
-- Name: ix_stock_count_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_count_lines_product_id ON public.stock_count_lines USING btree (product_id);


--
-- Name: ix_stock_count_lines_stock_count_id_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_stock_count_lines_stock_count_id_product_id ON public.stock_count_lines USING btree (stock_count_id, product_id);


--
-- Name: ix_stock_count_lines_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_count_lines_tenant_id ON public.stock_count_lines USING btree (tenant_id);


--
-- Name: ix_stock_counts_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_counts_location_id ON public.stock_counts USING btree (location_id);


--
-- Name: ix_stock_counts_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_counts_tenant_id ON public.stock_counts USING btree (tenant_id);


--
-- Name: ix_stock_counts_tenant_id_warehouse_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_counts_tenant_id_warehouse_id_status ON public.stock_counts USING btree (tenant_id, warehouse_id, status);


--
-- Name: ix_stock_counts_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_counts_warehouse_id ON public.stock_counts USING btree (warehouse_id);


--
-- Name: ix_stock_movements_created_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_movements_created_by_user_id ON public.stock_movements USING btree (created_by_user_id);


--
-- Name: ix_stock_movements_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_movements_location_id ON public.stock_movements USING btree (location_id);


--
-- Name: ix_stock_movements_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_movements_product_id ON public.stock_movements USING btree (product_id);


--
-- Name: ix_stock_movements_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_movements_tenant_id ON public.stock_movements USING btree (tenant_id);


--
-- Name: ix_stock_movements_tenant_id_product_id_occurred_at_utc; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_movements_tenant_id_product_id_occurred_at_utc ON public.stock_movements USING btree (tenant_id, product_id, occurred_at_utc);


--
-- Name: ix_stock_movements_tenant_id_source_document_type_source_docume; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_movements_tenant_id_source_document_type_source_docume ON public.stock_movements USING btree (tenant_id, source_document_type, source_document_id);


--
-- Name: ix_stock_movements_tenant_id_warehouse_id_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_movements_tenant_id_warehouse_id_location_id ON public.stock_movements USING btree (tenant_id, warehouse_id, location_id);


--
-- Name: ix_stock_movements_transfer_line_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_movements_transfer_line_id ON public.stock_movements USING btree (transfer_line_id);


--
-- Name: ix_stock_movements_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_stock_movements_warehouse_id ON public.stock_movements USING btree (warehouse_id);


--
-- Name: ix_suppliers_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_suppliers_tenant_id ON public.suppliers USING btree (tenant_id);


--
-- Name: ix_suppliers_tenant_id_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_suppliers_tenant_id_code ON public.suppliers USING btree (tenant_id, code);


--
-- Name: ix_suppliers_tenant_id_is_active; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_suppliers_tenant_id_is_active ON public.suppliers USING btree (tenant_id, is_active);


--
-- Name: ix_tenant_subscriptions_plan_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_tenant_subscriptions_plan_id ON public.tenant_subscriptions USING btree (plan_id);


--
-- Name: ix_tenant_subscriptions_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_tenant_subscriptions_tenant_id ON public.tenant_subscriptions USING btree (tenant_id);


--
-- Name: ix_tenant_subscriptions_tenant_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_tenant_subscriptions_tenant_id_status ON public.tenant_subscriptions USING btree (tenant_id, status);


--
-- Name: ix_tenants_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_tenants_code ON public.tenants USING btree (code);


--
-- Name: ix_tenants_name; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_tenants_name ON public.tenants USING btree (name);


--
-- Name: ix_transfer_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfer_lines_product_id ON public.transfer_lines USING btree (product_id);


--
-- Name: ix_transfer_lines_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfer_lines_tenant_id ON public.transfer_lines USING btree (tenant_id);


--
-- Name: ix_transfer_lines_transfer_id_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_transfer_lines_transfer_id_product_id ON public.transfer_lines USING btree (transfer_id, product_id);


--
-- Name: ix_transfer_operations_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfer_operations_tenant_id ON public.transfer_operations USING btree (tenant_id);


--
-- Name: ix_transfer_operations_tenant_id_transfer_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfer_operations_tenant_id_transfer_id ON public.transfer_operations USING btree (tenant_id, transfer_id);


--
-- Name: ix_transfer_operations_transfer_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfer_operations_transfer_id ON public.transfer_operations USING btree (transfer_id);


--
-- Name: ix_transfers_created_by_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfers_created_by_user_id ON public.transfers USING btree (created_by_user_id);


--
-- Name: ix_transfers_destination_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfers_destination_location_id ON public.transfers USING btree (destination_location_id);


--
-- Name: ix_transfers_destination_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfers_destination_warehouse_id ON public.transfers USING btree (destination_warehouse_id);


--
-- Name: ix_transfers_dispatch_operation_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_transfers_dispatch_operation_id ON public.transfers USING btree (dispatch_operation_id);


--
-- Name: ix_transfers_source_location_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfers_source_location_id ON public.transfers USING btree (source_location_id);


--
-- Name: ix_transfers_source_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfers_source_warehouse_id ON public.transfers USING btree (source_warehouse_id);


--
-- Name: ix_transfers_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfers_tenant_id ON public.transfers USING btree (tenant_id);


--
-- Name: ix_transfers_tenant_id_destination_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfers_tenant_id_destination_warehouse_id ON public.transfers USING btree (tenant_id, destination_warehouse_id);


--
-- Name: ix_transfers_tenant_id_source_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfers_tenant_id_source_warehouse_id ON public.transfers USING btree (tenant_id, source_warehouse_id);


--
-- Name: ix_transfers_tenant_id_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_transfers_tenant_id_status ON public.transfers USING btree (tenant_id, status);


--
-- Name: ix_units_of_measure_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_units_of_measure_tenant_id ON public.units_of_measure USING btree (tenant_id);


--
-- Name: ix_units_of_measure_tenant_id_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_units_of_measure_tenant_id_code ON public.units_of_measure USING btree (tenant_id, code);


--
-- Name: ix_units_of_measure_tenant_id_is_active; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_units_of_measure_tenant_id_is_active ON public.units_of_measure USING btree (tenant_id, is_active);


--
-- Name: ix_user_roles_role_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_user_roles_role_id ON public.user_roles USING btree (role_id);


--
-- Name: ix_user_roles_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_user_roles_tenant_id ON public.user_roles USING btree (tenant_id);


--
-- Name: ix_user_roles_tenant_id_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_user_roles_tenant_id_user_id ON public.user_roles USING btree (tenant_id, user_id);


--
-- Name: ix_user_roles_user_id_role_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_user_roles_user_id_role_id ON public.user_roles USING btree (user_id, role_id);


--
-- Name: ix_users_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_users_tenant_id ON public.users USING btree (tenant_id);


--
-- Name: ix_users_tenant_id_email; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_users_tenant_id_email ON public.users USING btree (tenant_id, email);


--
-- Name: ix_users_tenant_id_is_active; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_users_tenant_id_is_active ON public.users USING btree (tenant_id, is_active);


--
-- Name: ix_warehouse_locations_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_warehouse_locations_tenant_id ON public.warehouse_locations USING btree (tenant_id);


--
-- Name: ix_warehouse_locations_tenant_id_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_warehouse_locations_tenant_id_warehouse_id ON public.warehouse_locations USING btree (tenant_id, warehouse_id);


--
-- Name: ix_warehouse_locations_tenant_id_warehouse_id_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_warehouse_locations_tenant_id_warehouse_id_code ON public.warehouse_locations USING btree (tenant_id, warehouse_id, code);


--
-- Name: ix_warehouse_locations_tenant_id_warehouse_id_kind; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_warehouse_locations_tenant_id_warehouse_id_kind ON public.warehouse_locations USING btree (tenant_id, warehouse_id, kind);


--
-- Name: ix_warehouse_locations_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_warehouse_locations_warehouse_id ON public.warehouse_locations USING btree (warehouse_id);


--
-- Name: ix_warehouses_tenant_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_warehouses_tenant_id ON public.warehouses USING btree (tenant_id);


--
-- Name: ix_warehouses_tenant_id_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX ix_warehouses_tenant_id_code ON public.warehouses USING btree (tenant_id, code);


--
-- Name: ix_warehouses_tenant_id_is_active; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_warehouses_tenant_id_is_active ON public.warehouses USING btree (tenant_id, is_active);


--
-- Name: ix_warehouses_tenant_id_name; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX ix_warehouses_tenant_id_name ON public.warehouses USING btree (tenant_id, name);


--
-- Name: audit_logs fk_audit_logs_devices_device_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT fk_audit_logs_devices_device_id FOREIGN KEY (device_id) REFERENCES public.devices(id) ON DELETE SET NULL;


--
-- Name: audit_logs fk_audit_logs_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT fk_audit_logs_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: audit_logs fk_audit_logs_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT fk_audit_logs_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: cash_movements fk_cash_movements_cash_shifts_cash_shift_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cash_movements
    ADD CONSTRAINT fk_cash_movements_cash_shifts_cash_shift_id FOREIGN KEY (cash_shift_id) REFERENCES public.cash_shifts(id) ON DELETE CASCADE;


--
-- Name: cash_movements fk_cash_movements_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cash_movements
    ADD CONSTRAINT fk_cash_movements_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: cash_shifts fk_cash_shifts_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cash_shifts
    ADD CONSTRAINT fk_cash_shifts_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: change_log fk_change_log_devices_device_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.change_log
    ADD CONSTRAINT fk_change_log_devices_device_id FOREIGN KEY (device_id) REFERENCES public.devices(id) ON DELETE SET NULL;


--
-- Name: change_log fk_change_log_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.change_log
    ADD CONSTRAINT fk_change_log_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: change_log fk_change_log_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.change_log
    ADD CONSTRAINT fk_change_log_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: customers fk_customers_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT fk_customers_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: devices fk_devices_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.devices
    ADD CONSTRAINT fk_devices_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: devices fk_devices_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.devices
    ADD CONSTRAINT fk_devices_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: devices fk_devices_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.devices
    ADD CONSTRAINT fk_devices_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE SET NULL;


--
-- Name: pos_inventory_conflicts fk_pos_inventory_conflicts_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_inventory_conflicts
    ADD CONSTRAINT fk_pos_inventory_conflicts_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: pos_payments fk_pos_payments_pos_sales_pos_sale_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_payments
    ADD CONSTRAINT fk_pos_payments_pos_sales_pos_sale_id FOREIGN KEY (pos_sale_id) REFERENCES public.pos_sales(id) ON DELETE CASCADE;


--
-- Name: pos_payments fk_pos_payments_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_payments
    ADD CONSTRAINT fk_pos_payments_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: pos_sale_lines fk_pos_sale_lines_pos_sales_pos_sale_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_sale_lines
    ADD CONSTRAINT fk_pos_sale_lines_pos_sales_pos_sale_id FOREIGN KEY (pos_sale_id) REFERENCES public.pos_sales(id) ON DELETE CASCADE;


--
-- Name: pos_sale_lines fk_pos_sale_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_sale_lines
    ADD CONSTRAINT fk_pos_sale_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: pos_sales fk_pos_sales_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pos_sales
    ADD CONSTRAINT fk_pos_sales_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: price_lists fk_price_lists_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.price_lists
    ADD CONSTRAINT fk_price_lists_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: processed_sync_operations fk_processed_sync_operations_devices_device_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.processed_sync_operations
    ADD CONSTRAINT fk_processed_sync_operations_devices_device_id FOREIGN KEY (device_id) REFERENCES public.devices(id) ON DELETE RESTRICT;


--
-- Name: processed_sync_operations fk_processed_sync_operations_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.processed_sync_operations
    ADD CONSTRAINT fk_processed_sync_operations_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: processed_sync_operations fk_processed_sync_operations_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.processed_sync_operations
    ADD CONSTRAINT fk_processed_sync_operations_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE RESTRICT;


--
-- Name: product_barcodes fk_product_barcodes_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_barcodes
    ADD CONSTRAINT fk_product_barcodes_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE;


--
-- Name: product_barcodes fk_product_barcodes_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_barcodes
    ADD CONSTRAINT fk_product_barcodes_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: product_categories fk_product_categories_product_categories_parent_category_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_categories
    ADD CONSTRAINT fk_product_categories_product_categories_parent_category_id FOREIGN KEY (parent_category_id) REFERENCES public.product_categories(id) ON DELETE RESTRICT;


--
-- Name: product_categories fk_product_categories_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_categories
    ADD CONSTRAINT fk_product_categories_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: product_prices fk_product_prices_price_lists_price_list_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_prices
    ADD CONSTRAINT fk_product_prices_price_lists_price_list_id FOREIGN KEY (price_list_id) REFERENCES public.price_lists(id) ON DELETE CASCADE;


--
-- Name: product_prices fk_product_prices_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_prices
    ADD CONSTRAINT fk_product_prices_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: products fk_products_product_categories_category_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT fk_products_product_categories_category_id FOREIGN KEY (category_id) REFERENCES public.product_categories(id) ON DELETE SET NULL;


--
-- Name: products fk_products_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT fk_products_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: products fk_products_units_of_measure_unit_of_measure_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT fk_products_units_of_measure_unit_of_measure_id FOREIGN KEY (unit_of_measure_id) REFERENCES public.units_of_measure(id) ON DELETE RESTRICT;


--
-- Name: purchase_operations fk_purchase_operations_purchase_orders_purchase_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_operations
    ADD CONSTRAINT fk_purchase_operations_purchase_orders_purchase_order_id FOREIGN KEY (purchase_order_id) REFERENCES public.purchase_orders(id) ON DELETE CASCADE;


--
-- Name: purchase_operations fk_purchase_operations_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_operations
    ADD CONSTRAINT fk_purchase_operations_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: purchase_order_lines fk_purchase_order_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_order_lines
    ADD CONSTRAINT fk_purchase_order_lines_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: purchase_order_lines fk_purchase_order_lines_purchase_orders_purchase_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_order_lines
    ADD CONSTRAINT fk_purchase_order_lines_purchase_orders_purchase_order_id FOREIGN KEY (purchase_order_id) REFERENCES public.purchase_orders(id) ON DELETE CASCADE;


--
-- Name: purchase_order_lines fk_purchase_order_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_order_lines
    ADD CONSTRAINT fk_purchase_order_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: purchase_orders fk_purchase_orders_suppliers_supplier_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT fk_purchase_orders_suppliers_supplier_id FOREIGN KEY (supplier_id) REFERENCES public.suppliers(id) ON DELETE RESTRICT;


--
-- Name: purchase_orders fk_purchase_orders_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT fk_purchase_orders_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: purchase_orders fk_purchase_orders_users_created_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT fk_purchase_orders_users_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: purchase_orders fk_purchase_orders_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT fk_purchase_orders_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: purchase_orders fk_purchase_orders_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_orders
    ADD CONSTRAINT fk_purchase_orders_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: purchase_receipt_lines fk_purchase_receipt_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipt_lines
    ADD CONSTRAINT fk_purchase_receipt_lines_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: purchase_receipt_lines fk_purchase_receipt_lines_purchase_order_lines_purchase_order_l; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipt_lines
    ADD CONSTRAINT fk_purchase_receipt_lines_purchase_order_lines_purchase_order_l FOREIGN KEY (purchase_order_line_id) REFERENCES public.purchase_order_lines(id) ON DELETE RESTRICT;


--
-- Name: purchase_receipt_lines fk_purchase_receipt_lines_purchase_receipts_purchase_receipt_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipt_lines
    ADD CONSTRAINT fk_purchase_receipt_lines_purchase_receipts_purchase_receipt_id FOREIGN KEY (purchase_receipt_id) REFERENCES public.purchase_receipts(id) ON DELETE CASCADE;


--
-- Name: purchase_receipt_lines fk_purchase_receipt_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipt_lines
    ADD CONSTRAINT fk_purchase_receipt_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: purchase_receipts fk_purchase_receipts_purchase_orders_purchase_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipts
    ADD CONSTRAINT fk_purchase_receipts_purchase_orders_purchase_order_id FOREIGN KEY (purchase_order_id) REFERENCES public.purchase_orders(id) ON DELETE RESTRICT;


--
-- Name: purchase_receipts fk_purchase_receipts_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipts
    ADD CONSTRAINT fk_purchase_receipts_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: purchase_receipts fk_purchase_receipts_users_created_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipts
    ADD CONSTRAINT fk_purchase_receipts_users_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: purchase_receipts fk_purchase_receipts_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipts
    ADD CONSTRAINT fk_purchase_receipts_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: purchase_receipts fk_purchase_receipts_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_receipts
    ADD CONSTRAINT fk_purchase_receipts_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: purchase_return_lines fk_purchase_return_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_return_lines
    ADD CONSTRAINT fk_purchase_return_lines_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: purchase_return_lines fk_purchase_return_lines_purchase_order_lines_purchase_order_li; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_return_lines
    ADD CONSTRAINT fk_purchase_return_lines_purchase_order_lines_purchase_order_li FOREIGN KEY (purchase_order_line_id) REFERENCES public.purchase_order_lines(id) ON DELETE RESTRICT;


--
-- Name: purchase_return_lines fk_purchase_return_lines_purchase_returns_purchase_return_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_return_lines
    ADD CONSTRAINT fk_purchase_return_lines_purchase_returns_purchase_return_id FOREIGN KEY (purchase_return_id) REFERENCES public.purchase_returns(id) ON DELETE CASCADE;


--
-- Name: purchase_return_lines fk_purchase_return_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_return_lines
    ADD CONSTRAINT fk_purchase_return_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: purchase_returns fk_purchase_returns_purchase_orders_purchase_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_returns
    ADD CONSTRAINT fk_purchase_returns_purchase_orders_purchase_order_id FOREIGN KEY (purchase_order_id) REFERENCES public.purchase_orders(id) ON DELETE RESTRICT;


--
-- Name: purchase_returns fk_purchase_returns_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_returns
    ADD CONSTRAINT fk_purchase_returns_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: purchase_returns fk_purchase_returns_users_created_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_returns
    ADD CONSTRAINT fk_purchase_returns_users_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: purchase_returns fk_purchase_returns_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_returns
    ADD CONSTRAINT fk_purchase_returns_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: purchase_returns fk_purchase_returns_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.purchase_returns
    ADD CONSTRAINT fk_purchase_returns_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: refresh_tokens fk_refresh_tokens_devices_device_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT fk_refresh_tokens_devices_device_id FOREIGN KEY (device_id) REFERENCES public.devices(id) ON DELETE SET NULL;


--
-- Name: refresh_tokens fk_refresh_tokens_refresh_tokens_replaced_by_token_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT fk_refresh_tokens_refresh_tokens_replaced_by_token_id FOREIGN KEY (replaced_by_token_id) REFERENCES public.refresh_tokens(id) ON DELETE SET NULL;


--
-- Name: refresh_tokens fk_refresh_tokens_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT fk_refresh_tokens_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: refresh_tokens fk_refresh_tokens_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.refresh_tokens
    ADD CONSTRAINT fk_refresh_tokens_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: restaurant_open_orders fk_restaurant_open_orders_restaurant_tables_table_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.restaurant_open_orders
    ADD CONSTRAINT fk_restaurant_open_orders_restaurant_tables_table_id FOREIGN KEY (table_id) REFERENCES public.restaurant_tables(id) ON DELETE RESTRICT;


--
-- Name: restaurant_open_orders fk_restaurant_open_orders_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.restaurant_open_orders
    ADD CONSTRAINT fk_restaurant_open_orders_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: restaurant_tables fk_restaurant_tables_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.restaurant_tables
    ADD CONSTRAINT fk_restaurant_tables_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: role_permissions fk_role_permissions_permissions_permission_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.role_permissions
    ADD CONSTRAINT fk_role_permissions_permissions_permission_id FOREIGN KEY (permission_id) REFERENCES public.permissions(id) ON DELETE RESTRICT;


--
-- Name: role_permissions fk_role_permissions_roles_role_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.role_permissions
    ADD CONSTRAINT fk_role_permissions_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: role_permissions fk_role_permissions_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.role_permissions
    ADD CONSTRAINT fk_role_permissions_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: roles fk_roles_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT fk_roles_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: sales_operations fk_sales_operations_sales_orders_sales_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_operations
    ADD CONSTRAINT fk_sales_operations_sales_orders_sales_order_id FOREIGN KEY (sales_order_id) REFERENCES public.sales_orders(id) ON DELETE CASCADE;


--
-- Name: sales_operations fk_sales_operations_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_operations
    ADD CONSTRAINT fk_sales_operations_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: sales_order_lines fk_sales_order_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_order_lines
    ADD CONSTRAINT fk_sales_order_lines_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: sales_order_lines fk_sales_order_lines_sales_orders_sales_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_order_lines
    ADD CONSTRAINT fk_sales_order_lines_sales_orders_sales_order_id FOREIGN KEY (sales_order_id) REFERENCES public.sales_orders(id) ON DELETE CASCADE;


--
-- Name: sales_order_lines fk_sales_order_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_order_lines
    ADD CONSTRAINT fk_sales_order_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: sales_orders fk_sales_orders_customers_customer_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_orders
    ADD CONSTRAINT fk_sales_orders_customers_customer_id FOREIGN KEY (customer_id) REFERENCES public.customers(id) ON DELETE RESTRICT;


--
-- Name: sales_orders fk_sales_orders_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_orders
    ADD CONSTRAINT fk_sales_orders_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: sales_orders fk_sales_orders_users_created_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_orders
    ADD CONSTRAINT fk_sales_orders_users_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: sales_orders fk_sales_orders_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_orders
    ADD CONSTRAINT fk_sales_orders_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: sales_orders fk_sales_orders_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_orders
    ADD CONSTRAINT fk_sales_orders_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: sales_return_lines fk_sales_return_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_return_lines
    ADD CONSTRAINT fk_sales_return_lines_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: sales_return_lines fk_sales_return_lines_sales_order_lines_sales_order_line_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_return_lines
    ADD CONSTRAINT fk_sales_return_lines_sales_order_lines_sales_order_line_id FOREIGN KEY (sales_order_line_id) REFERENCES public.sales_order_lines(id) ON DELETE RESTRICT;


--
-- Name: sales_return_lines fk_sales_return_lines_sales_returns_sales_return_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_return_lines
    ADD CONSTRAINT fk_sales_return_lines_sales_returns_sales_return_id FOREIGN KEY (sales_return_id) REFERENCES public.sales_returns(id) ON DELETE CASCADE;


--
-- Name: sales_return_lines fk_sales_return_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_return_lines
    ADD CONSTRAINT fk_sales_return_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: sales_returns fk_sales_returns_sales_orders_sales_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_returns
    ADD CONSTRAINT fk_sales_returns_sales_orders_sales_order_id FOREIGN KEY (sales_order_id) REFERENCES public.sales_orders(id) ON DELETE RESTRICT;


--
-- Name: sales_returns fk_sales_returns_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_returns
    ADD CONSTRAINT fk_sales_returns_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: sales_returns fk_sales_returns_users_created_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_returns
    ADD CONSTRAINT fk_sales_returns_users_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: sales_returns fk_sales_returns_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_returns
    ADD CONSTRAINT fk_sales_returns_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: sales_returns fk_sales_returns_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_returns
    ADD CONSTRAINT fk_sales_returns_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: sales_shipment_lines fk_sales_shipment_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipment_lines
    ADD CONSTRAINT fk_sales_shipment_lines_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: sales_shipment_lines fk_sales_shipment_lines_sales_order_lines_sales_order_line_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipment_lines
    ADD CONSTRAINT fk_sales_shipment_lines_sales_order_lines_sales_order_line_id FOREIGN KEY (sales_order_line_id) REFERENCES public.sales_order_lines(id) ON DELETE RESTRICT;


--
-- Name: sales_shipment_lines fk_sales_shipment_lines_sales_shipments_sales_shipment_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipment_lines
    ADD CONSTRAINT fk_sales_shipment_lines_sales_shipments_sales_shipment_id FOREIGN KEY (sales_shipment_id) REFERENCES public.sales_shipments(id) ON DELETE CASCADE;


--
-- Name: sales_shipment_lines fk_sales_shipment_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipment_lines
    ADD CONSTRAINT fk_sales_shipment_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: sales_shipments fk_sales_shipments_sales_orders_sales_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipments
    ADD CONSTRAINT fk_sales_shipments_sales_orders_sales_order_id FOREIGN KEY (sales_order_id) REFERENCES public.sales_orders(id) ON DELETE RESTRICT;


--
-- Name: sales_shipments fk_sales_shipments_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipments
    ADD CONSTRAINT fk_sales_shipments_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: sales_shipments fk_sales_shipments_users_created_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipments
    ADD CONSTRAINT fk_sales_shipments_users_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: sales_shipments fk_sales_shipments_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipments
    ADD CONSTRAINT fk_sales_shipments_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: sales_shipments fk_sales_shipments_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_shipments
    ADD CONSTRAINT fk_sales_shipments_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: stock_adjustment_lines fk_stock_adjustment_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_adjustment_lines
    ADD CONSTRAINT fk_stock_adjustment_lines_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: stock_adjustment_lines fk_stock_adjustment_lines_stock_adjustments_stock_adjustment_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_adjustment_lines
    ADD CONSTRAINT fk_stock_adjustment_lines_stock_adjustments_stock_adjustment_id FOREIGN KEY (stock_adjustment_id) REFERENCES public.stock_adjustments(id) ON DELETE CASCADE;


--
-- Name: stock_adjustment_lines fk_stock_adjustment_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_adjustment_lines
    ADD CONSTRAINT fk_stock_adjustment_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: stock_adjustment_lines fk_stock_adjustment_lines_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_adjustment_lines
    ADD CONSTRAINT fk_stock_adjustment_lines_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: stock_adjustments fk_stock_adjustments_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_adjustments
    ADD CONSTRAINT fk_stock_adjustments_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: stock_adjustments fk_stock_adjustments_users_created_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_adjustments
    ADD CONSTRAINT fk_stock_adjustments_users_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: stock_adjustments fk_stock_adjustments_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_adjustments
    ADD CONSTRAINT fk_stock_adjustments_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: stock_balances fk_stock_balances_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_balances
    ADD CONSTRAINT fk_stock_balances_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: stock_balances fk_stock_balances_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_balances
    ADD CONSTRAINT fk_stock_balances_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: stock_balances fk_stock_balances_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_balances
    ADD CONSTRAINT fk_stock_balances_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: stock_balances fk_stock_balances_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_balances
    ADD CONSTRAINT fk_stock_balances_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: stock_count_lines fk_stock_count_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_count_lines
    ADD CONSTRAINT fk_stock_count_lines_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: stock_count_lines fk_stock_count_lines_stock_counts_stock_count_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_count_lines
    ADD CONSTRAINT fk_stock_count_lines_stock_counts_stock_count_id FOREIGN KEY (stock_count_id) REFERENCES public.stock_counts(id) ON DELETE CASCADE;


--
-- Name: stock_count_lines fk_stock_count_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_count_lines
    ADD CONSTRAINT fk_stock_count_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: stock_counts fk_stock_counts_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_counts
    ADD CONSTRAINT fk_stock_counts_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: stock_counts fk_stock_counts_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_counts
    ADD CONSTRAINT fk_stock_counts_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: stock_counts fk_stock_counts_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_counts
    ADD CONSTRAINT fk_stock_counts_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: stock_movements fk_stock_movements_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_movements
    ADD CONSTRAINT fk_stock_movements_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: stock_movements fk_stock_movements_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_movements
    ADD CONSTRAINT fk_stock_movements_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: stock_movements fk_stock_movements_users_created_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_movements
    ADD CONSTRAINT fk_stock_movements_users_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: stock_movements fk_stock_movements_warehouse_locations_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_movements
    ADD CONSTRAINT fk_stock_movements_warehouse_locations_location_id FOREIGN KEY (location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: stock_movements fk_stock_movements_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.stock_movements
    ADD CONSTRAINT fk_stock_movements_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: suppliers fk_suppliers_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.suppliers
    ADD CONSTRAINT fk_suppliers_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: tenant_change_counters fk_tenant_change_counters_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tenant_change_counters
    ADD CONSTRAINT fk_tenant_change_counters_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: tenant_subscriptions fk_tenant_subscriptions_plans_plan_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tenant_subscriptions
    ADD CONSTRAINT fk_tenant_subscriptions_plans_plan_id FOREIGN KEY (plan_id) REFERENCES public.plans(id) ON DELETE RESTRICT;


--
-- Name: transfer_lines fk_transfer_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfer_lines
    ADD CONSTRAINT fk_transfer_lines_products_product_id FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: transfer_lines fk_transfer_lines_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfer_lines
    ADD CONSTRAINT fk_transfer_lines_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: transfer_lines fk_transfer_lines_transfers_transfer_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfer_lines
    ADD CONSTRAINT fk_transfer_lines_transfers_transfer_id FOREIGN KEY (transfer_id) REFERENCES public.transfers(id) ON DELETE CASCADE;


--
-- Name: transfer_operations fk_transfer_operations_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfer_operations
    ADD CONSTRAINT fk_transfer_operations_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: transfer_operations fk_transfer_operations_transfers_transfer_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfer_operations
    ADD CONSTRAINT fk_transfer_operations_transfers_transfer_id FOREIGN KEY (transfer_id) REFERENCES public.transfers(id) ON DELETE CASCADE;


--
-- Name: transfers fk_transfers_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfers
    ADD CONSTRAINT fk_transfers_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: transfers fk_transfers_users_created_by_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfers
    ADD CONSTRAINT fk_transfers_users_created_by_user_id FOREIGN KEY (created_by_user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: transfers fk_transfers_warehouse_locations_destination_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfers
    ADD CONSTRAINT fk_transfers_warehouse_locations_destination_location_id FOREIGN KEY (destination_location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: transfers fk_transfers_warehouse_locations_source_location_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfers
    ADD CONSTRAINT fk_transfers_warehouse_locations_source_location_id FOREIGN KEY (source_location_id) REFERENCES public.warehouse_locations(id) ON DELETE RESTRICT;


--
-- Name: transfers fk_transfers_warehouses_destination_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfers
    ADD CONSTRAINT fk_transfers_warehouses_destination_warehouse_id FOREIGN KEY (destination_warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: transfers fk_transfers_warehouses_source_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.transfers
    ADD CONSTRAINT fk_transfers_warehouses_source_warehouse_id FOREIGN KEY (source_warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: units_of_measure fk_units_of_measure_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.units_of_measure
    ADD CONSTRAINT fk_units_of_measure_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: user_roles fk_user_roles_roles_role_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT fk_user_roles_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE RESTRICT;


--
-- Name: user_roles fk_user_roles_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT fk_user_roles_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: user_roles fk_user_roles_users_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT fk_user_roles_users_user_id FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: users fk_users_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT fk_users_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: warehouse_locations fk_warehouse_locations_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_locations
    ADD CONSTRAINT fk_warehouse_locations_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- Name: warehouse_locations fk_warehouse_locations_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_locations
    ADD CONSTRAINT fk_warehouse_locations_warehouses_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE CASCADE;


--
-- Name: warehouses fk_warehouses_tenants_tenant_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouses
    ADD CONSTRAINT fk_warehouses_tenants_tenant_id FOREIGN KEY (tenant_id) REFERENCES public.tenants(id) ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--


