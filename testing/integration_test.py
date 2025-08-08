#!/usr/bin/env python3
"""
Integration Test Suite for SaaS DocPay System
Tests all components and their interactions
"""

import requests
import json
import time
import sys

class DocPayTester:
    def __init__(self, base_url="http://localhost:5000"):
        self.base_url = base_url
        self.jwt_token = None
        self.test_results = []

    def log_test(self, test_name, success, message=""):
        """Log test results"""
        status = "✅ PASS" if success else "❌ FAIL"
        print(f"{status}: {test_name}")
        if message:
            print(f"   {message}")
        self.test_results.append({"test": test_name, "success": success, "message": message})

    def test_infrastructure(self):
        """Test infrastructure components"""
        print("\n🔧 Testing Infrastructure Components...")
        
        # Test API Gateway Health
        try:
            response = requests.get(f"{self.base_url}/health", timeout=10)
            self.log_test("API Gateway Health", response.status_code == 200, f"Status: {response.status_code}")
        except Exception as e:
            self.log_test("API Gateway Health", False, str(e))

        # Test Service Health Endpoints
        services = ["users", "payments", "notifications", "workflows"]
        for service in services:
            try:
                response = requests.get(f"{self.base_url}/health/{service}", timeout=10)
                self.log_test(f"{service.capitalize()} Service Health", response.status_code == 200, f"Status: {response.status_code}")
            except Exception as e:
                self.log_test(f"{service.capitalize()} Service Health", False, str(e))

    def test_api_gateway_routing(self):
        """Test API Gateway routing"""
        print("\n🌐 Testing API Gateway Routing...")
        
        # Test that protected endpoints return 401 without JWT
        endpoints = ["/api/user", "/api/payment", "/api/notification", "/api/workflow"]
        for endpoint in endpoints:
            try:
                response = requests.get(f"{self.base_url}{endpoint}", timeout=10)
                # Expecting 401 Unauthorized for protected endpoints
                success = response.status_code == 401
                self.log_test(f"Protected Route {endpoint}", success, f"Expected 401, got {response.status_code}")
            except Exception as e:
                self.log_test(f"Protected Route {endpoint}", False, str(e))

    def test_cors_configuration(self):
        """Test CORS configuration"""
        print("\n🔒 Testing CORS Configuration...")
        
        headers = {
            'Origin': 'http://localhost:4200',
            'Access-Control-Request-Method': 'GET',
            'Access-Control-Request-Headers': 'Content-Type'
        }
        
        try:
            response = requests.options(f"{self.base_url}/health", headers=headers, timeout=10)
            cors_headers = response.headers.get('Access-Control-Allow-Origin')
            self.log_test("CORS Preflight", response.status_code in [200, 204], f"Status: {response.status_code}")
            if cors_headers:
                self.log_test("CORS Headers Present", True, f"Allow-Origin: {cors_headers}")
            else:
                self.log_test("CORS Headers Present", False, "No CORS headers found")
        except Exception as e:
            self.log_test("CORS Configuration", False, str(e))

    def test_performance(self):
        """Test basic performance metrics"""
        print("\n⚡ Testing Performance...")
        
        start_time = time.time()
        try:
            response = requests.get(f"{self.base_url}/health", timeout=10)
            response_time = time.time() - start_time
            
            self.log_test("Response Time < 1s", response_time < 1.0, f"Response time: {response_time:.3f}s")
            self.log_test("Gateway Responds", response.status_code == 200, f"Status: {response.status_code}")
        except Exception as e:
            self.log_test("Performance Test", False, str(e))

    def test_service_discovery(self):
        """Test service discovery through gateway"""
        print("\n🔍 Testing Service Discovery...")
        
        # Test that gateway can reach all services
        services = {
            "users": "/health/users",
            "payments": "/health/payments", 
            "notifications": "/health/notifications",
            "workflows": "/health/workflows"
        }
        
        for service_name, endpoint in services.items():
            try:
                response = requests.get(f"{self.base_url}{endpoint}", timeout=10)
                self.log_test(f"Service Discovery: {service_name}", response.status_code == 200, f"Status: {response.status_code}")
            except Exception as e:
                self.log_test(f"Service Discovery: {service_name}", False, str(e))

    def run_all_tests(self):
        """Run all tests"""
        print("🚀 Starting SaaS DocPay System Integration Tests...")
        print("=" * 60)
        
        self.test_infrastructure()
        self.test_api_gateway_routing()
        self.test_cors_configuration()
        self.test_performance()
        self.test_service_discovery()
        
        # Summary
        print("\n" + "=" * 60)
        print("📊 TEST SUMMARY")
        print("=" * 60)
        
        total_tests = len(self.test_results)
        passed_tests = sum(1 for result in self.test_results if result["success"])
        failed_tests = total_tests - passed_tests
        
        print(f"Total Tests: {total_tests}")
        print(f"Passed: {passed_tests}")
        print(f"Failed: {failed_tests}")
        print(f"Success Rate: {(passed_tests/total_tests)*100:.1f}%")
        
        if failed_tests > 0:
            print("\n❌ Failed Tests:")
            for result in self.test_results:
                if not result["success"]:
                    print(f"   - {result['test']}: {result['message']}")
        
        return failed_tests == 0

if __name__ == "__main__":
    tester = DocPayTester()
    success = tester.run_all_tests()
    sys.exit(0 if success else 1)
