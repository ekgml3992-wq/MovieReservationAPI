pipeline {
    agent any

    triggers {
        githubPush()
    }

    stages {
        stage('Clean Solution') {
            steps {
                dir('MovieReservationAPI') {
                    sh 'dotnet clean MovieReservationAPI.sln'
                }
            }
        }

        stage('Restore') {
            steps {
                dir('MovieReservationAPI') {
                    sh 'dotnet restore MovieReservationAPI.sln'
                }
            }
        }

        stage('Build') {
            steps {
                dir('MovieReservationAPI') {
                    sh 'dotnet build MovieReservationAPI.sln --no-restore'
                }
            }
        }

        stage('Unit Test') {
            steps {
                dir('MovieReservationAPI') {
                    sh 'mkdir -p TestResults'
                    sh 'dotnet test MovieReservationAPI.Tests/MovieReservationAPI.Tests.csproj --no-build --logger "trx;LogFileName=unit-test-results.trx"'
                }
            }
        }

        stage('Start API Server') {
            steps {
                dir('MovieReservationAPI') {
                    sh 'nohup dotnet run --project MovieReservationAPI/MovieReservationAPI.csproj --urls=http://0.0.0.0:5000 > app.log 2>&1 &'
                }
            }
        }

        stage('Wait for Server') {
            steps {
                sh 'sleep 10'
            }
        }

        stage('Create Newman Environment') {
            steps {
                dir('MovieReservationAPI') {
                    sh '''
cat > test/jenkins_environment.json <<EOF
{
    "id": "jenkins-env",
    "name": "Jenkins",
    "values": [
        {
            "key": "baseUrl",
            "value": "http://127.0.0.1:5000",
            "type": "default",
            "enabled": true
        }
    ],
    "_postman_variable_scope": "environment"
}
EOF
'''
                }
            }
        }

        stage('Run Newman Tests') {
            steps {
                dir('MovieReservationAPI') {
                    sh 'newman run test/MovieReservationAPI.postman_collection.json -e test/jenkins_environment.json --insecure'
                }
            }
        }
    }

post {
    failure {
        mail to: 'ekgml3992@gmail.com',
             subject: "젠킨스 빌드 실패: ${env.JOB_NAME} #${env.BUILD_NUMBER}",
             body: """
Build failed.

Project: ${env.JOB_NAME}
Build Number: ${env.BUILD_NUMBER}

Check console output:
${env.BUILD_URL}
"""
    }

    fixed {
        mail to: 'ekgml3992@gmail.com',
             subject: "젠킨스 빌드 수정완료: ${env.JOB_NAME} #${env.BUILD_NUMBER}",
             body: """
The previously failing build has been fixed.

Project: ${env.JOB_NAME}
Build Number: ${env.BUILD_NUMBER}

Check build:
${env.BUILD_URL}
"""
    }

    always {
        dir('MovieReservationAPI') {
            sh "pkill -f 'dotnet run --project MovieReservationAPI/MovieReservationAPI.csproj' || true"
        }
        archiveArtifacts artifacts: 'MovieReservationAPI/app.log, MovieReservationAPI/test/jenkins_environment.json, MovieReservationAPI/**/*.trx', onlyIfSuccessful: false
    }
  }
}
